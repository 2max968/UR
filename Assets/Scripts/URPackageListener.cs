using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Assets.Scripts
{
    public class URPackageListener
    {
        TcpClient? client = null;
        NetworkStream? stream = null;
        Thread? listenThread = null;

        byte[] packageHeadBuffer = new byte[5];

        public RobotStatePackage_RobotModeData RobotModeData { get; private set; }
        public RobotStatePackage_JointData JointData { get; private set; }
        public RobotStatePackage_CartesianInfo CartesianInfo { get; private set; }
        public RobotStatePackage_MasterboardData MasterboardData { get; private set; }

        public bool Connected => client != null && client.Connected;

        public void Connect(string address, bool realtime = false)
        {
            var ip = IPAddress.Parse(address);
            var remote = new IPEndPoint(ip, realtime ? 30003 : 30001);
            client = new TcpClient();
            client.BeginConnect(ip, realtime ? 30003 : 30001, connectCallback, null);
        }

        public void Close()
        {
            client?.Close();
        }

        void connectCallback(IAsyncResult result)
        {
            if (client == null)
                throw new Exception();

            client.EndConnect(result);
            stream = client.GetStream();
            listenThread = new Thread(listen);
            listenThread.Start();
        }

        void listen()
        {
            if (client is null || stream is null)
                throw new Exception();

            while (client.Connected)
            {
                try
                {
                    stream.Read(packageHeadBuffer, 0, 5);
                    int packageLength = URUtil.ArrayToInt32(packageHeadBuffer, 0);
                    byte robotMessageType = packageHeadBuffer[4];

                    byte[] packageBuffer = new byte[packageLength - 5];
                    stream.Read(packageBuffer, 0, packageBuffer.Length);

                    int pointer = 0;
                    // MESSAGE_TYPE_ROBOT_STATE
                    if (robotMessageType == 16)
                    {
                        while (pointer < packageBuffer.Length)
                        {
                            int subPackageLength = URUtil.ArrayToInt32(packageBuffer, pointer);
                            byte subPackageType = packageBuffer[pointer + 4];
                            switch (subPackageType)
                            {
                                case 0: // Robot Mode Data
                                    RobotModeData = URUtil.ArrayToStruct<RobotStatePackage_RobotModeData>(packageBuffer, pointer + 5);
                                    break;
                                case 1: // JointData
                                    JointData = URUtil.ArrayToStruct<RobotStatePackage_JointData>(packageBuffer, pointer + 5);
                                    break;
                                case 4: // Cartesian Info
                                    CartesianInfo = URUtil.ArrayToStruct<RobotStatePackage_CartesianInfo>(packageBuffer, pointer + 5);
                                    break;
                                case 3: // Masterboard data
                                    MasterboardData =
                                        URUtil.ArrayToStruct<RobotStatePackage_MasterboardData>(packageBuffer,
                                            pointer + 5);
                                    break;
                            }
                            pointer += subPackageLength;
                        }
                    }
                }
                catch (System.IO.IOException) { }
            }
        }

        public void SendCommand(string command)
        {
            if (stream == null || client == null || !client.Connected)
                return;
            byte[] data = Encoding.UTF8.GetBytes(command + "\n");
            stream.Write(data, 0, data.Length);
        }
    }

    public struct RobotStatePackage_RobotModeData
    {
        public ulong timestamp;
        public NetworkBool isRealRobotConnected;
        public NetworkBool isRealRobotEnabled;
        public NetworkBool isRobotPowerOn;
        public NetworkBool isEmergencyStopped;
        public NetworkBool isProtectiveStopped;
        public NetworkBool isProgramRunning;
        public NetworkBool isProgramPaused;
        public RobotMode robotMode;
        public ControlMode controlMode;
        public NetworkDouble targetSpeedFraction;
        public NetworkDouble speedScaling;
        public NetworkDouble targetSpeedFractionLimit;
        public sbyte reserved;
    }

    public struct RobotStatePackage_JointData
    {
        public RobotStatePackage_SingleJointData j1;
        public RobotStatePackage_SingleJointData j2;
        public RobotStatePackage_SingleJointData j3;
        public RobotStatePackage_SingleJointData j4;
        public RobotStatePackage_SingleJointData j5;
        public RobotStatePackage_SingleJointData j6;

        public RobotStatePackage_SingleJointData[] AsArray => new[] { j1, j2, j3, j4, j5, j6 };
    }

    public struct RobotStatePackage_SingleJointData
    {
        public NetworkDouble q_actual;
        public NetworkDouble q_target;
        public NetworkDouble qd_actual;
        public NetworkFloat I_actual;
        public NetworkFloat V_actual;
        public NetworkFloat T_motor;
        public NetworkFloat T_micro;
        public byte jointMode;
    }

    public struct RobotStatePackage_CartesianInfo
    {
        public NetworkDouble X;
        public NetworkDouble Y;
        public NetworkDouble Z;
        public NetworkDouble Rx;
        public NetworkDouble Ry;
        public NetworkDouble Rz;
        public NetworkDouble TCPOffsetX;
        public NetworkDouble TCPOffsetY;
        public NetworkDouble TCPOffsetZ;
        public NetworkDouble TCPOffsetRx;
        public NetworkDouble TCPOffsetRy;
        public NetworkDouble TCPOffsetRz;
    }

    public struct RobotStatePackage_MasterboardData
    {
        public NetworkInt32 digitalInputBits;
        public NetworkInt32 digitalOutputBits;
        public byte analogInputRange0;
        public byte analogInputRange1;
        public NetworkDouble analogInput0;
        public NetworkDouble analogInput1;
        public sbyte analogOutputDomain0;
        public sbyte analogOutputDomain1;
        public NetworkDouble analogOutput0;
        public NetworkDouble analogOutput1;
        public NetworkFloat masterBoardtemeperature;
        public NetworkFloat robotVoltage48V;
        public NetworkFloat masterIOCurrent;
        public SafetyMode safetyMode;
        public NetworkBool inReducedMode;
    }

    public enum ControlMode : byte
    {
        Position = 0,
        Teach = 1,
        Force = 2,
        Torque = 3
    }

    public enum RobotMode : sbyte
    {
        NoController = -1,
        Disconnected = 0,
        ConfirmSafety = 1,
        Booting = 2,
        PowerOff = 3,
        PowerOn = 4,
        Idle = 5,
        Backdrive = 6,
        Running = 7,
        UpdatingFirmware = 8
    }

    public enum SafetyMode : byte
    {
        UndefinedSafetymode = 11,
        ValidateJointId = 10,
        Fault = 9,
        Violation = 8,
        RobotEmergencyStop = 7,
        SystemEmergencyStop = 6,
        SafeguardStop = 5,
        Recovery = 4,
        ProtectiveStop = 3,
        Reduced = 2,
        Normal = 1
    }
}
