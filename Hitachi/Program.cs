using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Csv;

namespace Hitachi
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the file name:");
            string fileName = Console.ReadLine();
            
            Console.WriteLine("Enter the sender email address:");
            string senderEmail = Console.ReadLine();
            
            Console.WriteLine("Enter the password:");
            string password = Console.ReadLine();
            
            Console.WriteLine("Enter the receiver email address:");
            string receiverEmail = Console.ReadLine();
            
            //Test with this File
            //var weatherData = new WeatherData("testData.csv");
            var weatherData = new WeatherData(fileName);
            var filteredData = weatherData.FilterData();
            if (filteredData.Count() == 0)
            {
                Console.WriteLine("Sorry, there are no good days for launching.");
                return;
            }
            //TODO: CSVWriter method!
            
            var emailService = new Email();
            emailService.SendEmail(receiverEmail, "RocketLaunch", "filteredData", senderEmail, password);
        }
    }

    public class WeatherData
    {
        private string fileName;
        private Dictionary<int, DayProps> dict;

        public WeatherData(string fileName)
        {
            this.fileName = fileName;
            dict = new Dictionary<int, DayProps>();
            var options = new CsvOptions()
            {
                HeaderMode = HeaderMode.HeaderPresent
            };
            var reader = File.ReadAllText(fileName);
            foreach (var csvLine in CsvReader.ReadFromText(reader, options))
            {
                for (int i = 1; i < csvLine.Values.Length; i++)
                {
                    var props = new DayProps();

                    if (dict.ContainsKey(i))
                    {
                        props = dict[i];
                    }
                    else
                    {
                        dict.Add(i, props);
                    }

                    var n = csvLine[i];

                    if (csvLine[0] == "Temperature")
                    {
                        props.Temperature = int.Parse(n);
                    }
                    else if (csvLine[0] == "Wind")
                    {
                        props.Wind = int.Parse(n);
                    }
                    else if (csvLine[0] == "Humidity")
                    {
                        props.Humidity = int.Parse(n);
                    }
                    else if (csvLine[0] == "Precipitation")
                    {
                        props.Precipitation = int.Parse(n);
                    }
                    else if (csvLine[0] == "Lightning")
                    {
                        props.Lightning = n;
                    }
                    else if (csvLine[0] == "Clouds")
                    {
                        props.Clouds = n;
                    }
                }
            }
        }

        public IEnumerable<DayProps> FilterData()
        {
            return dict
                .Where(x => x.Value.Temperature >= 2
                            && x.Value.Temperature <= 31
                            && x.Value.Wind <= 10
                            && x.Value.Humidity < 60
                            && x.Value.Precipitation == 0
                            && x.Value.Lightning == "No"
                            && (x.Value.Clouds != "Cumulus" && x.Value.Clouds != "Nimbus"))
                .Select(x => x.Value);
        }
        
        // public void WriteToCsv(Dictionary<int, DayProps> dict, string fileName)
        // {
        //     using (var writer = new StreamWriter(fileName))
        //     {
        //         writer.WriteLine("Temperature,Wind,Humidity,Precipitation,Lightning,Clouds");
        //         foreach (var kvp in dict)
        //         {
        //             var props = kvp.Value;
        //             var line = $"{props.Temperature},{props.Wind},{props.Humidity},{props.Precipitation},{props.Lightning},{props.Clouds}";
        //             writer.WriteLine(line);
        //         }
        //     }
        }
        
    }

    public class DayProps
    {
        public int Temperature { get; set; }

        public int Wind { get; set; }

        public int Humidity { get; set; }

        public int Precipitation { get; set; }

        public string Lightning { get; set; }

        public string Clouds { get; set; }
    }

    public class Email
    {
        // Trying GmailSender
        // public void SendMail(string sender, string senderPassword, string receiver, string data)
        // {
        //     // try
        //     // {
        //         MailMessage newMail = new MailMessage();
        //         // use the Gmail SMTP Host
        //         SmtpClient client = new SmtpClient("smtp.gmail.com");
        //
        //         // Follow the RFS 5321 Email Standard
        //         newMail.From = new MailAddress(sender, "Aleksandar");
        //
        //         newMail.To.Add(receiver); // declare the email subject
        //
        //         newMail.Subject = "Rocket Launch"; // use HTML for the email body
        //
        //         newMail.IsBodyHtml = true;
        //         newMail.Body = data.Split("\n\r").ToString();
        //
        //         // enable SSL for encryption across channels
        //         client.EnableSsl = true;
        //         // Port 465 for SSL communication
        //         client.Port = 465;
        //         // Provide authentication information with Gmail SMTP server to authenticate your sender account
        //         client.Credentials =
        //             new System.Net.NetworkCredential(sender, senderPassword);
        //
        //         client.Send(newMail); // Send the constructed mail
        //         Console.WriteLine("Email Sent");
        //     // }
        //     
        //     // catch (Exception ex)
        //     // {
        //     //     Console.WriteLine($"Error occured while sending the mail: {ex.Message}");
        //     // }
        // }

        public void SendEmail(string toEmail, string subject, string body, string fromEmail, string password)
        {
            var smtpClient = new SmtpClient("smtp.abv.bg", 465)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            var mailMessage = new MailMessage(fromEmail, toEmail, subject, body);
            mailMessage.IsBodyHtml = true;

            try
            {
                smtpClient.Send(mailMessage);
                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }
        }

    }

