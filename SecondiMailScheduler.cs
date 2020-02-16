using SecondiMailScheduler.Data;
using SecondiMailScheduler.Model;
using SecondiMailScheduler.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SecondiMailScheduler
{
    public partial class SecondiMailScheduler : ServiceBase
    {
        Timer timer = new Timer();

        // Main Settings
        private int _Interval = 1;
        private bool _EnableTestMode;

        // Connection Settings
        private string _Host;
        private int _Port;
        private string _User;
        private string _Password;
        private bool _EnableSSL;

        // Remitent Settings
        private string _From;
        private string _FromName;

        // Test-mode "To" address
        private string _To;

        public SecondiMailScheduler()
        {
            InitializeComponent();

            // Load main settings
            //_EnableTestMode = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableTestMode"]);
            //_Interval = Convert.ToInt32(ConfigurationManager.AppSettings["Interval"]);
            //_User = ConfigurationManager.AppSettings["User"];
            //_Password = ConfigurationManager.AppSettings["Password"];

            // Add "TestMode_" suffix if testmode is enabled
            //string prefix = (_EnableTestMode) ? "TestMode_" : "" ;

            // Get host, port and from address. If TestMode is enabled, the prefix "TestMode_" will be added to load the test values
            //_Host = ConfigurationManager.AppSettings[prefix + "Host"];
            //_Port = Convert.ToInt32(ConfigurationManager.AppSettings[prefix + "Port"]);
            //_From = ConfigurationManager.AppSettings[prefix + "From"];
            //_FromName = ConfigurationManager.AppSettings[prefix + "FromName"];

            // If test mode is enabled, add the "To" address. Set null otherwise
            //_To = (_EnableTestMode) ? ConfigurationManager.AppSettings[prefix + "To"] : null;

            Data.SettingsContext db = new Data.SettingsContext();

            Setting current = db.Settings.FirstOrDefault();

            WriteToFile("INFO", "Loading settings from database.");
            AddEvent(1, "[INFO]", "El servicio está cargando los parámetros de configuración.");

            _Host = current.Host;
            _Port = current.Port;
            _User = current.UserName;
            _FromName = current.UserDisplay;
            _EnableTestMode = current.TestMode;
            _EnableSSL = current.EnableSSL;
            _To = current.TestModeRecipient;
            _From = current.UserName;

            if (_EnableTestMode)
            {
                WriteToFile("INFO", "Running service in test-mode.");
                AddEvent(1, "[INFO]", "El servicio iniciará su ejecución en Test-Mode.");
            }
        }

        protected override void OnStart(string[] args)
        {
            WriteToFile("INFO", "Service started");
            AddEvent(1, "[INFO]", "El servicio ha comenzado su ejecución de manera correcta.");
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = _Interval*60*1000; // Number in miliseconds  
            timer.Enabled = true;
        }

        protected override void OnStop()
        {
            WriteToFile("INFO", "Service stopped");
            AddEvent(1, "[INFO]", "El servicio ha detenido su ejecución.");
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            AddEvent(1, "[INFO]", "El servicio ha retomado su ejecución satisfactoriamente.");
            SendMail();
        }

        public void SendMail()
        {
            try {

                //######################################
                // Get pending emails. ie. sent = false
                //######################################

                NoticesContext db = new NoticesContext();

                var DueNotices = from s in db.Notices
                                 select s;

                List<DueNotice> dueNotices = DueNotices.Where(s => s.Processed == false).ToList();

                SmtpClient smtpClient = new SmtpClient(_Host, _Port);
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential(_User, _Password);

                MailAddress From = new MailAddress(_From, _FromName, System.Text.Encoding.UTF8);

                WriteToFile("INFO", "Found "+ dueNotices.Count()+ " pending notice(s)");
                AddEvent(1, "[INFO]", "Se encontraron "+dueNotices.Count()+" envío(s) pendiente(s) programados.");

                bool ErrorFound = false;
                for (int i=0; i < dueNotices.Count(); i++) { 

                    try {

                        //######################################
                        //Send Email
                        //######################################

                        string recipient = _To;

                        if (!_EnableTestMode)
                            recipient = dueNotices.ElementAt(i).Recipients;

                        MailAddress To = new MailAddress(recipient);

                        MailMessage Message = new MailMessage(From, To);

                        Message.IsBodyHtml = true;

                        string Template = File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "template.html"));

                        Template = Template.Replace("@CompanyName@", dueNotices.ElementAt(i).Company);
                        Template = Template.Replace("@LocationName@", dueNotices.ElementAt(i).Location);
                        Template = Template.Replace("@ContentPersonal@", BuildAsList(dueNotices.ElementAt(i).ContentPersonal));
                        Template = Template.Replace("@ContentEmpresa@", BuildAsList(dueNotices.ElementAt(i).ContentEmpresa));
                        Template = Template.Replace("@ContentVehiculos@", BuildAsList(dueNotices.ElementAt(i).ContentVehiculos));
                        Message.Body = Template;
                        Message.Subject = "Aviso de vencimiento";

                        Message.BodyEncoding = System.Text.Encoding.UTF8;

                        smtpClient.Send(Message);

                        WriteToFile("INFO", "Notice with Id = " + dueNotices.ElementAt(i).Id+ " sent at succesfully");

                        //######################################
                        //Update the status of the email to sent
                        //######################################

                        NoticesContext SuccessDb = new NoticesContext();
                        SuccessDb.Notices.Find(dueNotices.ElementAt(i).Id).Processed = true;
                        SuccessDb.SaveChanges();

                        MailsContext mailsContext = new MailsContext();

                        MailSending newMail = new MailSending();
                        newMail.Comitent = dueNotices.ElementAt(i).Comitent;
                        newMail.Company = dueNotices.ElementAt(i).Company;
                        newMail.Location = dueNotices.ElementAt(i).Location;
                        newMail.Content = Template;
                        newMail.Recipients = _To;
                        newMail.SentOn = DateTime.Now;

                        mailsContext.MailsSendings.Add(newMail);
                        mailsContext.SaveChanges();

                    }
                    catch (SmtpFailedRecipientsException smtp_rec)
                    {
                        ErrorFound = true;
                        WriteToFile("ERROR", "Failed to send Notice with Id: " + dueNotices.ElementAt(i).Id);
                        WriteToFile("ERROR", "Notice was not delivered to all recipients.");
                        WriteToFile("EXCEPTION", smtp_rec.GetType().FullName + "|" + smtp_rec.Message + "|" + smtp_rec.StackTrace);

                    }
                    catch (SmtpException smtp_ex)
                    {
                        ErrorFound = true;
                        WriteToFile("ERROR", "Notice to send email with Id: " + dueNotices.ElementAt(i).Id);
                        WriteToFile("ERROR", "Notice not sent. An exception rised while attempting to send email");
                        WriteToFile("EXCEPTION",smtp_ex.GetType().FullName + "|" + smtp_ex.Message + "|" + smtp_ex.StackTrace);
                    }
                    catch(Exception ex)
                    {
                        ErrorFound = true;
                        WriteToFile("ERROR", "Notice to send email with Id: " + dueNotices.ElementAt(i).Id);
                        WriteToFile("ERROR", " Notice not sent. An exception rised while attempting to send email");
                        WriteToFile("EXCEPTION", ex.GetType().FullName + " | " + ex.Message + "|" + ex.StackTrace);
                    }

                }

                if (ErrorFound)
                {
                    AddEvent(3, "[WARNING]", "Ciertos correos no han podido enviarse. Se reintentará mas tarde.");
                }
                else
                {
                    AddEvent(2, "[SUCCESS]", "Todos los correos pendientes fueron enviados correctamente.");
                }
            }
            catch (Exception e)
            {
                //If an Exception is Found, write to the log
                AddEvent(4, "[ERROR]", "No se ha podido establecer una conexión con la base de datos. ");
                WriteToFile("ERROR", "An exception rised while attempting to access the Database");
                WriteToFile("EXCEPTION", e.GetType().FullName + "|" + e.Message + "|" + e.StackTrace);
            }
        }

        public static string BuildAsList(string raw)
        {
            if (raw == null || raw == "")
                return "";

            string main = "<ul>@Elements</ul>";

            string elementsAsString = "";

            string itemTemplate = "<li><i>@Doc</i> - @Date</li>";

            string[] Elements = raw.Split('#');

            foreach(string row in Elements)
            {
                string[] split = row.Split('$');
                string newItem = itemTemplate.Replace("@Date", split[0]).Replace("@Doc",split[1]);

                elementsAsString += newItem;
            }

            return main.Replace("@Elements", elementsAsString);
        }

        public void AddEvent(int code, string tag, string content)
        {
            Event newEvent = new Event();
            newEvent.Code = code;
            newEvent.Content = content;
            newEvent._TimeStamp = DateTime.Now;

            EventsContext db = new EventsContext();

            db.Events.Add(newEvent);

            db.SaveChanges();
        }

        public void WriteToFile(string tag, string content)
        {
            string Line = "[" + DateTime.Now + "] :: ["+ tag +"] :: "+content;
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to since it does not exists.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Line);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Line);
                }
            }
        }
    }
}