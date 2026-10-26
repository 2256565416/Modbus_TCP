using System;
using System.Linq;
using System.Net.Sockets;
using System.Windows;
using NModbus;

namespace ModbusMasterWPF
{
    public partial class MainWindow : Window
    {
        private TcpClient _tcpClient;
        private IModbusMaster _modbusMaster;

        private string Ip => txtIp.Text.Trim();
        private int Port => int.TryParse(txtPort.Text, out int p) ? p : 502;
        private byte SlaveId => byte.TryParse(txtSlaveId.Text, out byte id) ? id : (byte)1;

        public MainWindow()
        {
            InitializeComponent();
        }

        // 连接
        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _tcpClient = new TcpClient();
                _tcpClient.Connect(Ip, Port);

                var factory = new ModbusFactory();
                _modbusMaster = factory.CreateMaster(_tcpClient);

                AddLog("✅ 连接成功");
                btnConnect.IsEnabled = false;
                btnDisconnect.IsEnabled = true;
            }
            catch (Exception ex)
            {
                AddLog($"❌ 连接失败：{ex.Message}");
            }
        }

        // 断开
        private void BtnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _tcpClient?.Close();
                _modbusMaster?.Dispose();
                AddLog("🔌 已断开连接");
                btnConnect.IsEnabled = true;
                btnDisconnect.IsEnabled = false;
            }
            catch (Exception ex)
            {
                AddLog($"❌ 断开失败：{ex.Message}");
            }
        }

        // 读线圈
        private void BtnReadCoils_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckConnected()) return;
            try
            {
                ushort addr = ushort.Parse(txtStartAddress.Text);
                ushort count = ushort.Parse(txtReadCount.Text);
                bool[] coils = _modbusMaster.ReadCoils(SlaveId, addr, count);
                AddLog($"📖 线圈 [{addr}~{addr + count - 1}]：{string.Join(", ", coils)}");
            }
            catch (Exception ex) { AddLog($"❌ 读线圈失败：{ex.Message}"); }
        }

        // 读离散输入
        private void BtnReadInputs_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckConnected()) return;
            try
            {
                ushort addr = ushort.Parse(txtStartAddress.Text);
                ushort count = ushort.Parse(txtReadCount.Text);
                bool[] inputs = _modbusMaster.ReadInputs(SlaveId, addr, count);
                AddLog($"📖 离散输入 [{addr}~{addr + count - 1}]：{string.Join(", ", inputs)}");
            }
            catch (Exception ex) { AddLog($"❌ 读离散输入失败：{ex.Message}"); }
        }

        // 读保持寄存器
        private void BtnReadHoldingRegisters_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckConnected()) return;
            try
            {
                ushort addr = ushort.Parse(txtStartAddress.Text);
                ushort count = ushort.Parse(txtReadCount.Text);
                ushort[] regs = _modbusMaster.ReadHoldingRegisters(SlaveId, addr, count);
                AddLog($"📖 保持寄存器 [{addr}~{addr + count - 1}]：{string.Join(", ", regs)}");
            }
            catch (Exception ex) { AddLog($"❌ 读保持寄存器失败：{ex.Message}"); }
        }

        // 读输入寄存器
        private void BtnReadInputRegisters_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckConnected()) return;
            try
            {
                ushort addr = ushort.Parse(txtStartAddress.Text);
                ushort count = ushort.Parse(txtReadCount.Text);
                ushort[] regs = _modbusMaster.ReadInputRegisters(SlaveId, addr, count);
                AddLog($"📖 输入寄存器 [{addr}~{addr + count - 1}]：{string.Join(", ", regs)}");
            }
            catch (Exception ex) { AddLog($"❌ 读输入寄存器失败：{ex.Message}"); }
        }

        // 写单个线圈
        private void BtnWriteSingleCoil_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckConnected()) return;
            try
            {
                ushort addr = ushort.Parse(txtWriteAddress.Text);
                bool value = txtWriteValue.Text.Trim() == "1";
                _modbusMaster.WriteSingleCoil(SlaveId, addr, value);
                AddLog($"✏️ 写入线圈 {addr} = {value}");
            }
            catch (Exception ex) { AddLog($"❌ 写线圈失败：{ex.Message}"); }
        }

        // 写单个保持寄存器
        private void BtnWriteSingleRegister_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckConnected()) return;
            try
            {
                ushort addr = ushort.Parse(txtWriteAddress.Text);
                ushort value = ushort.Parse(txtWriteValue.Text);
                _modbusMaster.WriteSingleRegister(SlaveId, addr, value);
                AddLog($"✏️ 写入保持寄存器 {addr} = {value}");
            }
            catch (Exception ex) { AddLog($"❌ 写寄存器失败：{ex.Message}"); }
        }

        private bool CheckConnected()
        {
            if (_modbusMaster == null || _tcpClient?.Connected == false)
            {
                AddLog("⚠️ 请先连接 Modbus 服务器");
                return false;
            }
            return true;
        }

        private void AddLog(string msg)
        {
            Dispatcher.Invoke(() =>
            {
                listLog.Items.Add($"{DateTime.Now:HH:mm:ss} | {msg}");
                if (listLog.Items.Count > 0)
                    listLog.ScrollIntoView(listLog.Items[listLog.Items.Count - 1]);
            });
        }
    }
}