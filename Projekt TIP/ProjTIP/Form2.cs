using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

using Ozeki.Media.MediaHandlers;
using Ozeki.VoIP;
using Ozeki.VoIP.SDK;

namespace ProjTIP
{
    public partial class Form2 : Form
    {
        private string zalogowany;
        private string LocalIP;
        private static string DoceloweIP;

        private static Label labell5;
        private static Label labell6;
        private static Label labell7;

        ///////////////
        //// TCP do wstepnego polaczenia wysylanie
        private TcpClient client = new TcpClient();
        private NetworkStream mainStream;
        private int portNumber;

        ///////////////
        //// TCP do wstepnego polaczenia nasluchiwanie
        private TcpClient clientserv = new TcpClient();
        private TcpListener server;
        private NetworkStream netStream;
        private int portNumberServ;

        private readonly Thread Listetning;
        private readonly Thread GetMessage;

        ///////////////
        //// komunikaty
        string komZapros = "INV";
        string komOdmow = "ODM";
        string komAkceptuj = "AKC";

        bool czyRozmawia = false;

        //////////////////
        static ISoftPhone softphone;   // softphone object
        static IPhoneLine phoneLine;   // phoneline object
        static IPhoneCall call;
        static string caller;
        static Microphone microphone;
        static Speaker speaker;
        static PhoneCallAudioSender mediaSender;
        static PhoneCallAudioReceiver mediaReceiver;
        static MediaConnector connector;
        ///////////////////



        public Form2(string login)
        {
            
            zalogowany = login;

            Listetning = new Thread(StartListening);
            GetMessage = new Thread(OdczytajKomunikat);

            InitializeComponent();      

            /////////////
            //// parametry nasluchiwania tcp


            labell5 = label5;
            labell6 = label6;
            labell7 = label7;

            label2.Text = zalogowany;
            LocalIP = Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString();
            //LocalIP = "192.168.56.1";
            textBox1.Text = LocalIP;

            listView1.SmallImageList = imageList1;

            listView1.Items.Add("user1", 0);
            listView1.Items.Add("user2", 1);
            listView1.Items.Add("user3", 2);
            listView1.Items.Add("user4", 0);
            listView1.Items.Add("user5", 0);
   

            
        }

        private void StartListening()
        {
            while (!clientserv.Connected)
            {
                server.Start();
                clientserv = server.AcceptTcpClient();
            }
            GetMessage.Start();
        }

        private void StopListening()
        {
            server.Stop();
            clientserv = null;

            if (Listetning.IsAlive) Listetning.Abort();
            if (GetMessage.IsAlive) GetMessage.Abort();
        }

        private void OdczytajKomunikat()
        {
            BinaryFormatter binFormatter = new BinaryFormatter();

            if (clientserv.Connected)
            {
                netStream = clientserv.GetStream();
                string komunikat = (String)binFormatter.Deserialize(netStream);
                if (komunikat=="INV")
                {
                    string ktonazwa = Dns.GetHostEntry(((IPEndPoint)clientserv.Client.RemoteEndPoint).Address).HostName.ToString();
                    
                    IPEndPoint remoteIpEndPoint = clientserv.Client.RemoteEndPoint as IPEndPoint;
                    string kto = remoteIpEndPoint.Address.ToString();
                    DialogResult dialogResult = MessageBox.Show("Chcesz odebrać połączenie od: " + ktonazwa+" ?", "Some Title", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        //textBox2.Text = kto;
                        textBox2.Invoke(new MethodInvoker(delegate { textBox2.Text = kto; }));
                        //LocalIP = textBox1.Text;

                        Glowna(kto);
                        /*
                        clientserv.Close();
                        StopListening();
                        */
                        //WyslijKomunikat(komAkceptuj);
                        //Glowna(Dns.GetHostEntry(((IPEndPoint)clientserv.Client.RemoteEndPoint).Address).ToString());
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        MessageBox.Show("Polaczenie od: " + ktonazwa + " odrzucone");
                        clientserv.Close();
                        //WyslijKomunikat(komOdmow);
                        //clientserv.Close();
                    }
                }
                if(komunikat=="ODM")
                {
                    MessageBox.Show("Polaczenie zostalo odrzucone");
                }
                if(komunikat=="AKC")
                {
                    MessageBox.Show("Polaczenie zostalo zaakceptowane");
                }

            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            portNumberServ = 3003;
            server = new TcpListener(IPAddress.Any, portNumberServ);

            Listetning.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            StopListening();
        }

        private void WyslijKomunikat(string komunikat)
        {
            BinaryFormatter binFormatter = new BinaryFormatter();
            mainStream = client.GetStream();
            binFormatter.Serialize(mainStream, komunikat);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "")
            {
                MessageBox.Show("Wpisz adres docelowy");
            }
            else
            {
                bool czypolaczono = false;
                DoceloweIP = textBox2.Text;
                
                /////////////
                //// polaczenie tcp
                portNumber = 3003;

                client.Connect(DoceloweIP, portNumber);
                MessageBox.Show("Connected!");
                WyslijKomunikat(komZapros);
                /*if (client.Connected)
                {
                    client.Close();
                    czypolaczono = true;
                    StopListening();
                }
                if (czypolaczono == true)
                {
                    Glowna(DoceloweIP);
                }*/

                Glowna(DoceloweIP);
                /*try
                {
                    client.Connect(DoceloweIP, portNumber);
                    MessageBox.Show("Connected!");
                    WyslijKomunikat(komZapros);
                    Glowna(DoceloweIP);
                    //WyslijKomunikat(komAkceptuj);
                }
                catch (Exception)
                {
                    MessageBox.Show("Failed to connect...");
                }*/
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            DoceloweIP = textBox2.Text;
            if(textBox2.Text == "")
            {
                MessageBox.Show("Wpisz adres docelowy");
            }
            else
            {
                //////////////////////////
                
                //////////////////////////

            }
                Glowna(DoceloweIP);
        }

        void Glowna(string DocIP)
        {
            softphone = SoftPhoneFactory.CreateSoftPhone(5000, 10000);

            microphone = Microphone.GetDefaultDevice();
            speaker = Speaker.GetDefaultDevice();
            mediaSender = new PhoneCallAudioSender();
            mediaReceiver = new PhoneCallAudioReceiver();
            connector = new MediaConnector();

            DoceloweIP = DocIP;

            //Console.WriteLine("Please enter the IP address of your machine: ");
            //var ipAddress = LocalIP;
            var config = new DirectIPPhoneLineConfig(LocalIP, 5060);
            //var config = new DirectIPPhoneLineConfig(textBox1.Text, 5060);
            phoneLine = softphone.CreateDirectIPPhoneLine(config);
            phoneLine.RegistrationStateChanged += line_RegStateChanged;
            softphone.IncomingCall += softphone_IncomingCall;
            softphone.RegisterPhoneLine(phoneLine);

            //Console.ReadLine();
        }

        private static void line_RegStateChanged(object sender, RegistrationStateChangedArgs e)
        {

            if (e.State == RegState.NotRegistered || e.State == RegState.Error)
            {
                //labell5.Text = "Registration failed!";
                labell5.Invoke(new MethodInvoker(delegate { labell5.Text = "Registration failed!"; }));
                
            }
                //Console.WriteLine("Registration failed!");
                

            if (e.State == RegState.RegistrationSucceeded)
            {
                //labell6.Text = "Online!";
                labell6.Invoke(new MethodInvoker(delegate { labell6.Text = "Online!"; }));
                //Console.WriteLine("Registration succeeded - Online!");
                //Console.WriteLine("Enter the IP address of the destination: ");

                string ipToDial = DoceloweIP;
                StartCall(ipToDial);
            }
        }

        private static void StartCall(string numberToDial)
        {
            if (call == null)
            {
                call = softphone.CreateDirectIPCallObject(phoneLine, new DirectIPDialParameters("5060"), numberToDial);
                call.CallStateChanged += call_CallStateChanged;

                call.Start();
            }
        }

        static void softphone_IncomingCall(object sender, VoIPEventArgs<IPhoneCall> e)
        {
            call = e.Item;
            caller = call.DialInfo.CallerID;
            
            /*DialogResult dialogResult = MessageBox.Show("Chcesz odebrać połączenie od: "+caller.ToString(), "Some Title", MessageBoxButtons.YesNo);
            if(dialogResult == DialogResult.Yes)
            {
                call.CallStateChanged += call_CallStateChanged;
                call.Answer();
            }
            else if (dialogResult == DialogResult.No)
            {
                MessageBox.Show("Polaczenie od: " + caller.ToString()+" odrzucone");
                CloseDevices();
            }*/
            
            //MessageBox.Show("Chcesz odebrac polaczenie od: " + caller.ToString());
            call.CallStateChanged += call_CallStateChanged;
            call.Answer();
            
        }

        static void call_CallStateChanged(object sender, CallStateChangedArgs e)
        {
            //Console.WriteLine("Call state: {0}.", e.State);
            //labell7.Text = ""+ e.State;
            labell7.Invoke(new MethodInvoker(delegate { labell7.Text = "" + e.State; }));
            if (e.State == CallState.Answered)
                SetupDevices();

            if (e.State.IsCallEnded())
                CloseDevices();
        }

        static void SetupDevices()
        {
            microphone.Start();
            connector.Connect(microphone, mediaSender);

            speaker.Start();
            connector.Connect(mediaReceiver, speaker);

            mediaSender.AttachToCall(call);
            mediaReceiver.AttachToCall(call);
        }

        static void CloseDevices()
        {
            microphone.Dispose();
            speaker.Dispose();

            mediaReceiver.Detach();
            mediaSender.Detach();
            connector.Dispose();
        }

        private void button16_Click(object sender, EventArgs e)
        {
            
        }

        // klawiatura numeryczna
        private void button1_Click(object sender, EventArgs e)
        {
            textBox2.Text += "1";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox2.Text += "2";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox2.Text += "3";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox2.Text += "4";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            textBox2.Text += "5";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            textBox2.Text += "6";
        }

        private void button7_Click(object sender, EventArgs e)
        {
            textBox2.Text += "7";
        }

        private void button8_Click(object sender, EventArgs e)
        {
            textBox2.Text += "8";
        }

        private void button9_Click(object sender, EventArgs e)
        {
            textBox2.Text += "9";
        }

        private void button10_Click(object sender, EventArgs e)
        {
            textBox2.Text += ".";
        }

        private void button11_Click(object sender, EventArgs e)
        {
            textBox2.Text += "0";
        }

        
        /////////////////////////////////

       
    }
}
