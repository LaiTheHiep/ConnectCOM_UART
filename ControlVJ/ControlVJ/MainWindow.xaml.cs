using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ControlVJ
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window, INotifyPropertyChanged
  {
    public MainWindow()
    {
      InitializeComponent();

      SetupMain();
      this.DataContext = this;
      threadReceive();
    }

    private List<TestUART> ListTestUART = new List<TestUART>()
    {
      new TestUART("TEST,1", "Vào chế độ Debug, mỗi phiên 60s"),
      new TestUART("TEST,3", "Test Flash"),
      new TestUART("TEST,5", "Kiểm tra GPS"),
      new TestUART("TEST,6", "Kiểm tra thời gian thiết bị"),
      new TestUART("TEST,7", "Kiểm tra nháy đèn 3 lần"),
      new TestUART("TEST,8", "Kiểm tra đèn sáng 3s"),
      new TestUART("TEST,9", "Test Duty"),
      new TestUART("TEST,10", "Kiểm tra Operation Time"),
      new TestUART("TEST,11", "Kiểm tra MOVEDISTANCE"),
      new TestUART("TEST,14", "Reset thiết bị"),
      new TestUART("TEST,15", "Test GSM"),
      new TestUART("TEST,18", "Kiểm tra điện áp Solar, PIN"),
      new TestUART("TEST,20", "Test gửi bản tin định kì lên Server"),
      new TestUART("TEST,21", "Kiểm tra dòng tiêu thụ"),
      new TestUART("TEST,22", "Test ALARM"),
      new TestUART("TEST,23", "Test cấu hình"),
      new TestUART("TEST,26", "Test Watchdog ngoài (gửi lệnh sau vài giây thiết bị phải tự reset)"),
      new TestUART("TEST,27", "Xem các giá trị điện áp ADC (solar, pin, photocell, Iload), gửi lại lệnh lần nữa để dừng test"),
      new TestUART("TEST,30", "Thời gian trạng thái OFF"),
      new TestUART("TEST,52", "Bật chế độ test GPS"),
      new TestUART("TEST,53", "Tắt chế độ Debug GPS"),
      new TestUART("TEST,54", "Đảo chế độ debug GPS"),
      new TestUART("TEST,56", "Bật chê độ debug GSM"),
      new TestUART("TEST,57", "Tắt chế độ Debug GSM"),
      new TestUART("TEST,59", "Bật chế độ debug Flash"),
      new TestUART("TEST,60", "Vào chế độ Low Power"),
      new TestUART("TEST,100", "Xóa thời gian hoạt động."),
      new TestUART("TEST,102", "Đóng cổng TCP"),
      new TestUART("TEST,103", "update fw"),
      new TestUART("TEST,201", "Lâý IMEI Module Sim"),
      new TestUART("TEST,202", "Lấy SIM IMEI"),
      new TestUART("TEST,203", "Lấy cường độ sóng."),
      new TestUART("TEST,204", "Lấy điện áp PIN mặt trời'"),
      new TestUART("TEST,205", "Lấy điện áp ACQUY"),
      new TestUART("TEST,206", "Lấy dong LED"),
      new TestUART("TEST,207", "Lấy ADC quang trở"),
      new TestUART("TEST,208", "TEST GSM"),
      new TestUART("TEST,209", "TEST FLASH"),
      new TestUART("TEST,210", "TEST GPS")
    };

    private string[] Ports = SerialPort.GetPortNames();

    private SerialPort P = new SerialPort();

    private string NewLine = Environment.NewLine;

    private bool isConnect = false;

    private int countTimeOut = 0;

    private string _DataReceiveUART = "";
    public string DataReceiveUART
    {
      get { return _DataReceiveUART; }
      set
      {
        if (value != _DataReceiveUART)
        {
          _DataReceiveUART = value;
          OnPropertyChanged("DataReceiveUART");
        }
      }
    }

    private string _DataShow = "";
    public string DataShow
    {
      get { return _DataShow; }
      set
      {
        if (value != _DataShow)
        {
          _DataShow = value;
          OnPropertyChanged("DataShow");
        }
      }
    }

    //private string DataReceiveUART = "";
    //private string DataShow = "";

    private int TimeOut(DateTime time1, DateTime time2)
    {
      int h = time2.Hour - time1.Hour;
      int m = time2.Minute - time1.Minute;
      int s = time2.Second - time1.Second;
      int distance = h * 60 * 60 + m * 60 + s;

      return distance;
    }

    private void ConnectCOM(SerialPort COM, string portName)
    {
      COM.PortName = portName;
      COM.BaudRate = int.Parse(txtBaud.Text);
      COM.Open();
      SendAT(COM, "TEST,1");
    }

    private void DisconnectCOM(SerialPort COM)
    {
      COM.Close();
    }

    private void SendAT(SerialPort COM, string at)
    {
      if (COM.IsOpen)
      {
        COM.WriteLine(at);
        //txtDataShow.Text += at + NewLine;
        //txtDataResult.Text += at + NewLine;
        DataShow += at + NewLine;
      }
      else
      {
        MessageBox.Show("Kiểm tra cổng COM kết nối");
      }
    }

    private string ReceiveData(SerialPort COM)
    {
      string result = COM.ReadExisting();
      txtDataShow.Text += result + NewLine;
      txtDataResult.Text += result + NewLine;
      return result;
    }

    private void threadReceive()
    {
      new Thread(
        () =>
        {
          while (true)
          {
            if (P.IsOpen)
            {
              string temp = P.ReadExisting();

              if (temp != "")
              {
                this.DataReceiveUART += temp;
                this.DataShow += temp;
                countTimeOut = 0;
              }
              else
              {
                if(countTimeOut > 20)
                {
                  SendAT(P, "TEST,1");
                  countTimeOut = 0;
                }
                else
                {
                  countTimeOut++;
                }
              }
              Thread.Sleep(500);
            }
          }
        })
      { IsBackground = true }.Start();
    }

    private void SETUART(string dataSend, string pattReceive, int lenghtList, TextBlock tbStatus)
    {
      SendAT(P, dataSend);
      Thread.Sleep(1000);
      int dem = 0;
      DateTime time1 = DateTime.Now;
      string data = "";
      do
      {
        data = ReceiveData(P);
        if (data != "")
          break;
        dem = TimeOut(time1, DateTime.Now);
      } while (dem < 10);
      if (data != "")
      {
        var lsregex = Regex.Matches(data, pattReceive, RegexOptions.Singleline);
        if (lsregex != null && lsregex.Count == lenghtList)
        {
          tbStatus.Text = "Thành công";
          tbStatus.Foreground = new SolidColorBrush(Colors.Green);
        }
        else
        {
          tbStatus.Text = "Thất bại";
          tbStatus.Foreground = new SolidColorBrush(Colors.Red);
        }
      }
      else
      {
        tbStatus.Text = "Thất bại";
        tbStatus.Foreground = new SolidColorBrush(Colors.Red);
      }
    }

    private string GETUART(string dataSend, string pattReceive, int lenghtList, TextBlock tbStatus)
    {
      SendAT(P, dataSend);
      Thread.Sleep(1000);
      int dem = 0;
      DateTime time1 = DateTime.Now;
      string data = "";
      do
      {
        data = ReceiveData(P);
        if (data != "")
          break;
        dem = TimeOut(time1, DateTime.Now);
      } while (dem < 10);
      if (data != "")
      {
        var lsregex = Regex.Matches(data, pattReceive, RegexOptions.Singleline);
        if (lsregex != null && lsregex.Count == lenghtList)
        {
          tbStatus.Text = "Thành công";
          tbStatus.Foreground = new SolidColorBrush(Colors.Green);
          return data;
        }
        else
        {
          tbStatus.Text = "Thất bại";
          tbStatus.Foreground = new SolidColorBrush(Colors.Red);
          return "";
        }
      }
      else
      {
        tbStatus.Text = "Thất bại";
        tbStatus.Foreground = new SolidColorBrush(Colors.Red);
        return "";
      }
      return "";
    }

    private void SendUART(string data)
    {
      SendAT(P, data);
      Thread.Sleep(1000);
      ReceiveData(P);

    }

    private void SetupMain()
    {
      cbCOM.ItemsSource = Ports;
      lsvTest.ItemsSource = ListTestUART;
    }

    private void BtnSendAlamCode_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string patt = $"Thiet lap cau hinh 1: {txtAlamCode.Text}|ID thiet bi: {txtAlamCode.Text}";
        string datasend = $"SET,1,({txtAlamCode.Text})";
        //SETUART(datasend, patt, 2, sAlamCode);
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnGetAlamCode_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string patt = $"ID thiet bi:(.*?)";
        string datasend = $"GET,1";
        //string data = GETUART(datasend, patt, 1, sAlamCode);
        //var lsregex = Regex.Matches(data, patt, RegexOptions.Singleline);
        //int position = data.IndexOf(lsregex[0].ToString());
        //txtAlamCode.Text = data.Substring(position + lsregex[0].Length + 1, data.Length - (position + lsregex[0].Length + 1));
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnSetPassword_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string patt = $"Thiet lap cau hinh 2: {txtPassword.Text}|Password: {txtPassword.Text}";
        string datasend = $"SET,2,({txtPassword.Text})";
        //SETUART(datasend, patt, 2, sPassword);
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnGetPassword_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string patt = $"Password:(.*?)";
        string datasend = $"GET,2";
        //string data = GETUART(datasend, patt, 1, sPassword);
        //var lsregex = Regex.Matches(data, patt, RegexOptions.Singleline);
        //int position = data.IndexOf(lsregex[0].ToString());
        //txtPassword.Text = data.Substring(position + lsregex[0].Length + 1, data.Length - (position + lsregex[0].Length + 1));
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnSetSignalType_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string patt = $"Thiet lap cau hinh 3: {txtSignalType.Text}|Blink Type: {txtSignalType.Text}";
        string datasend = $"SET,3,({txtSignalType.Text})";
        //SETUART(datasend, patt, 2, sSignalType);
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnGetSignalType_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string patt = $"Blink Type:(.*?)";
        string datasend = $"GET,3";
        //string data = GETUART(datasend, patt, 1, sSignalType);
        //var lsregex = Regex.Matches(data, patt, RegexOptions.Singleline);
        //int position = data.IndexOf(lsregex[0].ToString());
        //txtSignalType.Text = data.Substring(position + lsregex[0].Length + 1, data.Length - (position + lsregex[0].Length + 1));
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void SetDistance_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string patt = $"Thiet lap cau hinh 4: {txtDistance.Text}|Distance: {txtDistance.Text}";
        string datasend = $"SET,4,({txtDistance.Text})";
        //SETUART(datasend, patt, 2, sDistance);
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void GetDistance_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string patt = $"Distance:(.*?)";
        string datasend = $"GET,4";
        //string data = GETUART(datasend, patt, 1, sDistance);
        //var lsregex = Regex.Matches(data, patt, RegexOptions.Singleline);
        //int position = data.IndexOf(lsregex[0].ToString());
        //txtDistance.Text = data.Substring(position + lsregex[0].Length + 1, data.Length - (position + lsregex[0].Length + 1));
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnSetLightLevel_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string patt = $"Thiet lap cau hinh 5: {cbLightLevel.Text}|Capacity: {cbLightLevel.Text}";
        string datasend = $"SET,5,({cbLightLevel.Text})";
        //SETUART(datasend, patt, 2, sLightLevel);
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnGetLightLevel_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string patt = $"Capacity:(.*?)";
        string datasend = $"GET,5";
        //string data = GETUART(datasend, patt, 1, sLightLevel);
        //var lsregex = Regex.Matches(data, patt, RegexOptions.Singleline);
        //int position = data.IndexOf(lsregex[0].ToString());
        //cbLightLevel.Text = data.Substring(position + lsregex[0].Length + 1, data.Length - (position + lsregex[0].Length + 1));
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnSetGPS_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string patt = $"Thiet lap cau hinh 6: {cbGPS.Text}|GPS Mode: {cbGPS.Text}|GPS Power:(.*?)";
        string datasend = $"SET,6,({cbLightLevel.Text})";
        //SETUART(datasend, patt, 3, sGPS);
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnGetGPS_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string patt = $"GPS Mode:(.*?)";
        string datasend = $"GET,6";
        //string data = GETUART(datasend, patt, 1, sGPS);
        //var lsregex = Regex.Matches(data, patt, RegexOptions.Singleline);
        //int position = data.IndexOf(lsregex[0].ToString());
        //cbGPS.Text = data.Substring(position + lsregex[0].Length + 1, data.Length - (position + lsregex[0].Length + 1));
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnSetPhoneVJ_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string patt = $"Thiet lap cau hinh 7: {txtPhoneVJ.Text}|SMS No1: {txtPhoneVJ.Text}";
        string datasend = $"SET,7,({txtPhoneVJ.Text})";
        //SETUART(datasend, patt, 2, sPhoneVJ);
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnGetPhoneVJ_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string patt = $"SMS No1:(.*?)";
        string datasend = $"GET,7";
        //string data = GETUART(datasend, patt, 1, sPhoneVJ);
        //var lsregex = Regex.Matches(data, patt, RegexOptions.Singleline);
        //int position = data.IndexOf(lsregex[0].ToString());
        //txtPhoneVJ.Text = data.Substring(position + lsregex[0].Length + 1, data.Length - (position + lsregex[0].Length + 1));
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnSetPhoneManager_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string patt = $"Thiet lap cau hinh 8: {txtPhoneManager.Text}|SMS No2: {txtPhoneManager.Text}";
        string datasend = $"SET,8,({txtPhoneManager.Text})";
        //SETUART(datasend, patt, 2, sPhoneManager);
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnGetPhoneManager_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string patt = $"SMS No2:(.*?)";
        string datasend = $"GET,8";
        //string data = GETUART(datasend, patt, 1, sPhoneManager);
        //var lsregex = Regex.Matches(data, patt, RegexOptions.Singleline);
        //int position = data.IndexOf(lsregex[0].ToString());
        //txtPhoneManager.Text = data.Substring(position + lsregex[0].Length + 1, data.Length - (position + lsregex[0].Length + 1));
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnSetPhoneSever_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string patt = $"Thiet lap cau hinh 9: {txtPhoneSever.Text}|SMS No3: {txtPhoneSever.Text}";
        string datasend = $"SET,9,({txtPhoneSever.Text})";
        //SETUART(datasend, patt, 2, sPhoneSever);
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnGetPhoneSever_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string patt = $"SMS No3:(.*?)";
        string datasend = $"GET,9";
        //string data = GETUART(datasend, patt, 1, sPhoneSever);
        //var lsregex = Regex.Matches(data, patt, RegexOptions.Singleline);
        //int position = data.IndexOf(lsregex[0].ToString());
        //txtPhoneSever.Text = data.Substring(position + lsregex[0].Length + 1, data.Length - (position + lsregex[0].Length + 1));
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnSetLocation_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string patt = $"Thiet lap cau hinh 10:(.*?)|Position:(.*?)";
        string datasend = $"SET,10,({txtLocation.Text})";
        //SETUART(datasend, patt, 2, sLocation);
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnGetLocation_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string patt = $"Position:(.*?)";
        string datasend = $"GET,10";
        //string data = GETUART(datasend, patt, 1, sLocation);
        //var lsregex = Regex.Matches(data, patt, RegexOptions.Singleline);
        //int position = data.IndexOf(lsregex[0].ToString());
        //txtLocation.Text = data.Substring(position + lsregex[0].Length + 1, data.Length - (position + lsregex[0].Length + 1));
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnSetTypeSever_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string patt = $"Thiet lap cau hinh 12:(.*?)|Server:(.*?)";
        string datasend = $"SET,12,({cbTypeSever.Text})";
        //SETUART(datasend, patt, 2, sTypeSever);
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnGetTypeSever_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string patt = $"Server:(.*?)";
        string datasend = $"GET,12";
        //string data = GETUART(datasend, patt, 1, sTypeSever);
        //var lsregex = Regex.Matches(data, patt, RegexOptions.Singleline);
        //int position = data.IndexOf(lsregex[0].ToString());
        //cbTypeSever.Text = data.Substring(position + lsregex[0].Length + 1, data.Length - (position + lsregex[0].Length + 1));
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnSetLight_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        int value = 0;
        if (ckLight.IsChecked == true)
          value = 1;

        string patt = $"Thiet lap cau hinh 17: {value}|Den Cau:(.*?)";
        string datasend = $"SET,17,({value})";
        //SETUART(datasend, patt, 2, sLight);
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnGetLight_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string patt = $"Den Cau:(.*?)";
        string datasend = $"GET,17";
        //string data = GETUART(datasend, patt, 1, sLight);
        //var lsregex = Regex.Matches(data, patt, RegexOptions.Singleline);
        //int position = data.IndexOf(lsregex[0].ToString());
        //string result = data.Substring(position + lsregex[0].Length + 1, data.Length - (position + lsregex[0].Length + 1));
        //if (result == "ON")
        //  ckLight.IsChecked = true;
        //else
        //  ckLight.IsChecked = false;
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnAlamMove_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        int value = 0;
        if (ckAlamMove.IsChecked == true)
          value = 1;

        string patt = $"Thiet lap cau hinh 20: {value}|Move alarm:(.*?)";
        string datasend = $"SET,20,({value})";
        //SETUART(datasend, patt, 2, sAlamMove);
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnGetAlamMove_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string patt = $"Move alarm:(.*?)";
        string datasend = $"GET,20";
        //string data = GETUART(datasend, patt, 1, sAlamMove);
        //var lsregex = Regex.Matches(data, patt, RegexOptions.Singleline);
        //int position = data.IndexOf(lsregex[0].ToString());
        //string result = data.Substring(position + lsregex[0].Length + 1, data.Length - (position + lsregex[0].Length + 1));
        //if (result == "1")
        //  ckLight.IsChecked = true;
        //else
        //  ckLight.IsChecked = false;
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnSetAlamAcquy_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        int value = 0;
        if (ckAlamAcquy.IsChecked == true)
          value = 1;

        string patt = $"Thiet lap cau hinh 21: {value}|Accu alarm:(.*?)";
        string datasend = $"SET,21,({value})";
        //SETUART(datasend, patt, 2, sAlamAcquy);
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnGetAlamAcquy_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string patt = $"Accu alarm:(.*?)";
        string datasend = $"GET,21";
        //string data = GETUART(datasend, patt, 1, sAlamAcquy);
        //var lsregex = Regex.Matches(data, patt, RegexOptions.Singleline);
        //int position = data.IndexOf(lsregex[0].ToString());
        //string result = data.Substring(position + lsregex[0].Length + 1, data.Length - (position + lsregex[0].Length + 1));
        //if (result == "1")
        //  ckAlamAcquy.IsChecked = true;
        //else
        //  ckAlamAcquy.IsChecked = false;
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnAlamLight_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        int value = 0;
        if (ckAlamLight.IsChecked == true)
          value = 1;

        string patt = $"Thiet lap cau hinh 22: {value}|LED alarm:(.*?)";
        string datasend = $"SET,22,({value})";
        //SETUART(datasend, patt, 2, sAlamLight);
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnGetAlamLight_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string patt = $"LED alarm:(.*?)";
        string datasend = $"GET,22";
        //string data = GETUART(datasend, patt, 1, sAlamLight);
        //var lsregex = Regex.Matches(data, patt, RegexOptions.Singleline);
        //int position = data.IndexOf(lsregex[0].ToString());
        //string result = data.Substring(position + lsregex[0].Length + 1, data.Length - (position + lsregex[0].Length + 1));
        //if (result == "1")
        //  ckAlamLight.IsChecked = true;
        //else
        //  ckAlamLight.IsChecked = false;
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnSetSetting_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string[] arr = txtSetting.Text.Split(':');
        string patt = $"Thiet lap cau hinh 201: {arr[0]},{arr[1]}|Alter Host: {txtSetting.Text}";
        string datasend = $"SET,201,({arr[0]},{arr[1]})";
        //SETUART(datasend, patt, 2, sSetting);
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void BtnGetSetting_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string patt = $"Alter Host:(.*?)";
        string datasend = $"GET,201";
        //string data = GETUART(datasend, patt, 1, sSetting);
        //var lsregex = Regex.Matches(data, patt, RegexOptions.Singleline);
        //int position = data.IndexOf(lsregex[0].ToString());
        //txtSetting.Text = data.Substring(position + lsregex[0].Length + 1, data.Length - (position + lsregex[0].Length + 1));
        SendAT(P, datasend);
      }
      catch
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng COM");
      }
    }

    private void SetAllSetting_Click(object sender, RoutedEventArgs e)
    {
      if (P.IsOpen)
      {
        MessageBox.Show("Đang load dữ liệu. Click OK và chờ đợi");
        BtnSendAlamCode_Click(sender, e); Thread.Sleep(1000);
        BtnSetPassword_Click(sender, e); Thread.Sleep(1000);
        BtnSetSignalType_Click(sender, e); Thread.Sleep(1000);
        SetDistance_Click(sender, e); Thread.Sleep(1000);
        BtnSetLightLevel_Click(sender, e); Thread.Sleep(1000);
        BtnSetGPS_Click(sender, e); Thread.Sleep(1000);
        BtnSetPhoneVJ_Click(sender, e); Thread.Sleep(1000);
        BtnSetPhoneManager_Click(sender, e); Thread.Sleep(1000);
        BtnSetPhoneSever_Click(sender, e); Thread.Sleep(1000);
        BtnSetLocation_Click(sender, e); Thread.Sleep(1000);
        BtnSetTypeSever_Click(sender, e); Thread.Sleep(1000);
        BtnSetLight_Click(sender, e); Thread.Sleep(1000);
        BtnAlamMove_Click(sender, e); Thread.Sleep(1000);
        BtnSetAlamAcquy_Click(sender, e); Thread.Sleep(1000);
        BtnAlamLight_Click(sender, e); Thread.Sleep(1000);
        BtnSetSetting_Click(sender, e); Thread.Sleep(1000);
        MessageBox.Show("Đã cài đặt hết tất cả các giá trị");
      }
    }

    private void GetAllSetting_Click(object sender, RoutedEventArgs e)
    {
      if (P.IsOpen)
      {
        MessageBox.Show("Đang load dữ liệu. Click OK và chờ đợi");
        BtnGetAlamCode_Click(sender, e); Thread.Sleep(1000);
        BtnGetPassword_Click(sender, e); Thread.Sleep(1000);
        BtnGetSignalType_Click(sender, e); Thread.Sleep(1000);
        GetDistance_Click(sender, e); Thread.Sleep(1000);
        BtnGetLightLevel_Click(sender, e); Thread.Sleep(1000);
        BtnGetGPS_Click(sender, e); Thread.Sleep(1000);
        BtnGetPhoneVJ_Click(sender, e); Thread.Sleep(1000);
        BtnGetPhoneManager_Click(sender, e); Thread.Sleep(1000);
        BtnGetPhoneSever_Click(sender, e); Thread.Sleep(1000);
        BtnGetLocation_Click(sender, e); Thread.Sleep(1000);
        BtnGetTypeSever_Click(sender, e); Thread.Sleep(1000);
        BtnGetLight_Click(sender, e); Thread.Sleep(1000);
        BtnGetAlamMove_Click(sender, e); Thread.Sleep(1000);
        BtnGetAlamAcquy_Click(sender, e); Thread.Sleep(1000);
        BtnGetAlamLight_Click(sender, e); Thread.Sleep(1000);
        BtnGetSetting_Click(sender, e); Thread.Sleep(1000);
        MessageBox.Show("Đã lấy hết tất cả các giá trị");
      }
    }

    private void BtnConnect_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (!isConnect)
        {
          ConnectCOM(P, cbCOM.Text);
          btnConnect.Content = "DisConnect";
          SlbCOM.Text = cbCOM.Text + " Connected";
          isConnect = !isConnect;
        }
        else
        {
          P.Close();
          btnConnect.Content = "Connect";
          SlbCOM.Text = "Kết nối với cổng COM";
          isConnect = !isConnect;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng kết nối");
      }
    }

    private void BtnReLoadCOM_Click(object sender, RoutedEventArgs e)
    {
      BtnConnect_Click(sender, e);
      cbCOM.ItemsSource = SerialPort.GetPortNames();
    }

    private void BtnConnect2_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (!isConnect)
        {
          ConnectCOM(P, cbCOM2.Text);
          btnConnect.Content = "DisConnect";
          SlbCOM.Text = cbCOM2.Text + " Connected";
          isConnect = !isConnect;
        }
        else
        {
          P.Close();
          btnConnect.Content = "Connect";
          SlbCOM.Text = "Kết nối với cổng COM";
          isConnect = !isConnect;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Lỗi kết nối. Vui lòng kiểm tra lại cổng kết nối");
      }
    }

    private void BtnSendData1_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string data = txtSendData1.Text;
        if (cbNewLine1.IsChecked == true)
        {
          data += NewLine;
        }
        //SendUART(data);
        SendAT(P, data);
      }
      catch
      {
        MessageBox.Show("Kiểm tra lại kết nối cổng COM");
      }
    }

    private void BtnSendData2_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string data = txtSendData2.Text;
        if (cbNewLine2.IsChecked == true)
        {
          data += NewLine;
        }
        //SendUART(data);
        SendAT(P, data);
      }
      catch
      {
        MessageBox.Show("Kiểm tra lại kết nối cổng COM");
      }
    }

    private void BtnSendData3_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string data = txtSendData3.Text;
        if (cbNewLine3.IsChecked == true)
        {
          data += NewLine;
        }
        //SendUART(data);
        SendAT(P, data);
      }
      catch
      {
        MessageBox.Show("Kiểm tra lại kết nối cổng COM");
      }
    }

    private void BtnTestUART_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        TestUART uart = (sender as Button).DataContext as TestUART;
        //SendUART(uart.CommandUART);
        SendAT(P, uart.CommandUART);
      }
      catch
      {
        MessageBox.Show("Kiểm tra lại kết nối cổng COM");
      }
      
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string newName)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(newName));
      }
    }

    private void TxtDataResult_TextChanged(object sender, TextChangedEventArgs e)
    {
      txtDataResult.ScrollToEnd();
    }

    private void TxtDataShow_TextChanged(object sender, TextChangedEventArgs e)
    {
      txtDataShow.ScrollToEnd();
    }
  }
}
