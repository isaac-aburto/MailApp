using MailApp.Models;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using OpenPop;
using OpenPop.Mime;
using OpenPop.Pop3;
using System.Web;
using MimeKit;

namespace MailApp
{
    class Program
    {
        static void Main(string[] args)
        {
            GetFinalRedirect("https://mandrillapp.com/track/click/31067670/us-west-2b.online.tableau.com?p=eyJzIjoieGt4NGlXMm1OUmdTOTJOLTlnR3ozSUIzNmVNIiwidiI6MSwicCI6IntcInVcIjozMTA2NzY3MCxcInZcIjoxLFwidXJsXCI6XCJodHRwczpcXFwvXFxcL3VzLXdlc3QtMmIub25saW5lLnRhYmxlYXUuY29tXFxcL3RcXFwvYmFja3NwYWNlc3BhXFxcL3ZpZXdzXFxcL1NtYXJ0c2hlZXR2MlxcXC9TaGVldDE_Om9yaWdpbmFsX3ZpZXc9eSY6ZGV2aWNlPXRhYmxldFwiLFwiaWRcIjpcIjhlZTJkMzQ5YWZmNTQ5OTNiYTk1MGEzZDBmNTBkMjcyXCIsXCJ1cmxfaWRzXCI6W1wiOGVlMjYzM2UzNTJlYjkzN2VlZDJiNzhmYWNhYzE4MmMyODkyZDdlMlwiXX0ifQ");
            Console.WriteLine("------ Aplicación Gmail ------");
            var Usuario = ConfigurationManager.AppSettings["Usuario"];
            var Password = ConfigurationManager.AppSettings["Password"];
            var ListaNegra = ConfigurationManager.AppSettings["ListaNegra"];
            Console.WriteLine("Lista negra: " + ListaNegra);
            //var lista = FetchAllMessages(Usuario, Password);
            ExtraerMensajesNoLeidos(Usuario, Password, ListaNegra);
            
            Console.WriteLine("------ Fin proceso ------");
            Environment.Exit(1);
            Console.ReadLine();
        }
        /*List<Message> listaUids*/
        private static void ExtraerMensajesNoLeidos(string Usuario, string Password, string ListaNegra)
        {
            try
            {
                using (var client = new ImapClient())
                {
                    //Conexión a Librería Gmail
                    client.Connect("imap.gmail.com", 993, true);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate(Usuario, Password);
                    System.Console.WriteLine("Conectando a " + Usuario);

                    //Obtención de Correos 
                    client.Inbox.Open(FolderAccess.ReadWrite);
                    var uids = client.Inbox.Search(SearchQuery.NotSeen);
                    
                    System.Console.WriteLine("Se encontraron " + uids.Count().ToString() + " correos sin leer");
                    var cont = 0;
                    var fileName = "";
                    //Recorreido de cada Id de Correo
                    foreach (var uid in uids)
                    {
                        
                        //Obtención de correo como Objeto
                        var message = client.Inbox.GetMessage(uid);

                        //foreach (var idmen in listaUids)
                        //{
                        //    if (message.MessageId == idmen.Headers.MessageId) {
                        //        Console.WriteLine("!!!!!");
                        //    }
                        //}
                        cont++;
                        System.Console.WriteLine("Analizando correo n° " + cont.ToString());

                        //Control de ListaNegra
                        var splitListaNegra = ListaNegra.Split(';');
                        bool pasa = true;
                        foreach (var from in splitListaNegra) {
                            if (message.From.ToString().Contains(from)) {
                                pasa = false;
                            }
                        }

                        // --- Crear Respuesta --- //
                        if (message.Subject.Contains("Ticket #") && pasa)
                        {
                            //Contiene RE en su Subject?
                            var RespuestaSplit = message.Subject.Split(':');
                            if (RespuestaSplit[0] == "RE" || RespuestaSplit[0] == "Re")
                            {
                                System.Console.WriteLine("Se encontró una respuesta para el portal...");

                                // Manejo de Archivos en el correo
                                var hasAttachments = message.BodyParts.Any(x => x.IsAttachment);
                                foreach (var attachment in message.Attachments)
                                {
                                    //fileName = attachment.ContentDisposition.FileName;
                                    fileName = "\n El mensaje contenía un archivo adjunto, pero no se pudo cargar.";
                                }

                                // Control de de datos importantes: dirección, idticket 
                                string from = message.From.ToString();
                                var splitFrom = from.Split('<');
                                var splitFromFrom = splitFrom[1].Split('>');
                                string correo = splitFromFrom[0];
                                var splitSub = message.Subject.Split('#');
                                var splitSubSub = splitSub[1].Split('-');
                                int idTicketcorreo = int.Parse(splitSubSub[0]);
                                int perfil = 5;
                                string nombreCreador = "";
                                //formato "\"Isaac 0000253000\" <finn_bisal@hotmail.com>"
                                System.Console.WriteLine("Creando respuesta de " + from + ", para el ticket #" + idTicketcorreo.ToString());

                                // Conexión a la Base de datos
                                using (ModeloPortal4Entities conexionDB = new ModeloPortal4Entities())
                                {
                                    int IDticket = conexionDB.Ticket.Where(w => w.Usuario.Correo == correo).Select(s => s.id_Ticket).FirstOrDefault();
                                    string RespuestaNueva = "";
                                    //Comprobar si el usuario existe.
                                    var usuarioHay = conexionDB.Usuario.Where(w => w.Correo == correo).ToList();
                                    var ultimoid = "0";
                                    if (usuarioHay.Count != 0)
                                    {
                                        Usuario usuario = conexionDB.Usuario.Where(w => w.Correo == correo).FirstOrDefault();

                                        HistorialRespuesta respuesta = new HistorialRespuesta();
                                        //Se asignan los atributos
                                        respuesta.id_Usuarios = usuario.id_Usuarios;
                                        respuesta.id_Ticket = idTicketcorreo;
                                        if (message.HtmlBody != null)
                                        {
                                            respuesta.Respuesta = message.HtmlBody.ToString() + fileName;
                                            RespuestaNueva = message.HtmlBody.ToString() + fileName;
                                        }
                                        else
                                        {
                                            respuesta.Respuesta = message.TextBody.ToString() + fileName;
                                            RespuestaNueva = message.TextBody.ToString() + fileName;
                                        }
                                        var fecha = DateTime.Now;
                                        respuesta.FechaRespuesta = fecha.AddHours(-2);

                                        // Agregar Estado al Ticket
                                        Ticket ticket = conexionDB.Ticket.Find(idTicketcorreo);
                                        var Estado = 1;
                                        ticket.Visto = false;
                                        if (usuario.EsBs == true)
                                        {

                                            //Agregar respuesta de cerrado 
                                            if (Estado == 2)
                                            {

                                                //Se setean las demas respeustas en false
                                                List<HistorialRespuesta> listahistorialRespuesta = conexionDB.HistorialRespuesta.Where(w => w.id_Ticket == idTicketcorreo).ToList();
                                                foreach (HistorialRespuesta historial in listahistorialRespuesta)
                                                {
                                                    historial.RespuestaCerrado = false;
                                                }
                                                //se agrega la respuesta seleccionada como la correcta:
                                                respuesta.RespuestaCerrado = true;
                                            }
                                        }
                                        else
                                        {
                                            Estado = 1;
                                        }
                                        ticket.id_EstadoTicket = Estado;
                                        respuesta.id_EstadoTicket = Estado;
                                        // Agregar Estado al Ticket

                                        ticket.Visto = false;
                                        ticket.UltimaRespuesta = fecha.AddHours(-2);
                                        conexionDB.HistorialRespuesta.Add(respuesta);
                                        conexionDB.SaveChanges();
                                        ultimoid = ticket.id_Ticket.ToString();
                                        perfil = int.Parse(usuario.id_Perfil.ToString());
                                        nombreCreador = usuario.Nombre;

                                    }
                                    //Crear un nuevo usuario
                                    else
                                    {
                                        Ticket ticket = conexionDB.Ticket.Find(idTicketcorreo);
                                        Usuario usuario = new Usuario();
                                        usuario.Correo = correo;
                                        var NombreCompleto = splitFrom[0].Split(' ');
                                        usuario.Nombre = NombreCompleto[0];
                                        usuario.Apellido = NombreCompleto[1];
                                        usuario.id_Empresa = ticket.Usuario.Empresa.id_Empresa;
                                        usuario.id_Perfil = 6;
                                        conexionDB.Usuario.Add(usuario);
                                        System.Console.WriteLine("El usuario no se encontró, pero se agregó al portal.");
                                        HistorialRespuesta respuesta = new HistorialRespuesta();
                                        //Se asignan los atributos
                                        respuesta.id_Usuarios = usuario.id_Usuarios;
                                        respuesta.id_Ticket = idTicketcorreo;
                                        
                                        if (message.HtmlBody != null)
                                        {
                                            respuesta.Respuesta = message.HtmlBody.ToString() + fileName;
                                            RespuestaNueva = message.HtmlBody.ToString() + fileName;
                                        }
                                        else
                                        {
                                            respuesta.Respuesta = message.TextBody.ToString() + fileName;
                                            RespuestaNueva = message.TextBody.ToString() + fileName;
                                        }
                                        var fecha = DateTime.Now;
                                        respuesta.FechaRespuesta = fecha.AddHours(-2);

                                        // Agregar Estado al Ticket

                                        var Estado = 1;
                                        ticket.Visto = false;
                                        if (usuario.EsBs == true)
                                        {

                                            //Agregar respuesta de cerrado 
                                            if (Estado == 2)
                                            {

                                                //Se setean las demas respeustas en false
                                                List<HistorialRespuesta> listahistorialRespuesta = conexionDB.HistorialRespuesta.Where(w => w.id_Ticket == idTicketcorreo).ToList();
                                                foreach (HistorialRespuesta historial in listahistorialRespuesta)
                                                {
                                                    historial.RespuestaCerrado = false;
                                                }
                                                //se agrega la respuesta seleccionada como la correcta:
                                                respuesta.RespuestaCerrado = true;

                                            }

                                        }
                                        else
                                        {
                                            Estado = 1;
                                        }
                                        ticket.id_EstadoTicket = Estado;
                                        respuesta.id_EstadoTicket = Estado;
                                        // Agregar Estado al Ticket

                                        ticket.Visto = false;
                                        ticket.UltimaRespuesta = fecha.AddHours(-2);
                                        conexionDB.HistorialRespuesta.Add(respuesta);
                                        conexionDB.SaveChanges();
                                        ultimoid = ticket.id_Ticket.ToString();
                                        perfil = int.Parse(usuario.id_Perfil.ToString());
                                        nombreCreador = usuario.Nombre;
                                    }
                                    string titulo = "Ticket #" + ultimoid + "- Portal Backspace";
                                    string titulo2 = "Ticket #" + ultimoid + "- Portal Backspace";
                                    string nombre = "Isaac Aburto";
                                    string telefono = "22222222";
                                    string celu = "+56953306060";

                                    string perfilUsuario = conexionDB.Perfil.Where(w => w.id_Perfil == perfil).Select(s => s.nomPerfil).FirstOrDefault().ToString();
                                    string descripcion = RespuestaNueva;
                                    string Respondedorr = String.Empty;
                                    Respondedorr = splitFromFrom[1];
                                    string asunto = message.Subject;
                                    string urlServidor = "http://help.backspace.cl/Ticket/GestionTicket/" + ultimoid;

                                    string textoCorreoCliente = System.IO.File.ReadAllText("C:\\Users\\PC\\source\\repos\\MailApp\\MailApp\\Styles\\RespuestaTicketCliente.html").Replace("[Nombre]", Respondedorr).Replace("[NombreCreador]", nombreCreador).Replace("[Perfil]]", perfilUsuario).Replace("[NombreCreador]", nombreCreador).Replace("[telefono]", telefono).Replace("[celu]", celu).Replace("[Asunto]", asunto).Replace("[IdTicket]", ultimoid).Replace("[Respuesta]", descripcion).Replace("[URLServidor]", urlServidor);
                                    EnviarMail(textoCorreoCliente, correo, titulo, "", "");

                                    string textoCorreo = System.IO.File.ReadAllText("C:\\Users\\PC\\source\\repos\\MailApp\\MailApp\\Styles\\RespuestaTicket.html").Replace("[Nombre]", nombre).Replace("[NombreCreador]", nombreCreador).Replace("[telefono]", telefono).Replace("[celu]", celu).Replace("[Respondedor]", Respondedorr).Replace("[Asunto]", asunto).Replace("[IdTicket]", ultimoid).Replace("[URLServidor]", urlServidor).Replace("[Respuesta]", descripcion);
                                    EnviarMail(textoCorreo, "soporte@backspace.cl", titulo2, "", "");

                                    client.Inbox.AddFlags(uid, MessageFlags.Seen, true);
                                    System.Console.WriteLine("Respuesta agregada");
                                    System.Console.WriteLine("--------------------------");
                                }

                            }

                        }

                        // --- Crear Ticket --- // 

                        else if (pasa)
                        {
                            var RespuestaSplit = message.Subject.Split(':');                  
                            //Crear Ticket
                            if (RespuestaSplit[0] != "RE" && RespuestaSplit[0] != "Re")
                            {
                                System.Console.WriteLine("Se encontró un correo para el portal...");
                                // CREAR TICKET
                                string correo = ObtenerCorreo(message.From.ToString());
                                using (ModeloPortal4Entities conexionDB = new ModeloPortal4Entities())
                                {
                                    // Manejo de Archivos en el correo
                                    var hasAttachments = message.BodyParts.Any(x => x.IsAttachment);
                                    foreach (var attachment in message.Attachments)
                                    {
                                        //fileName = attachment.ContentDisposition.FileName;
                                        fileName = "\n El mensaje contenía un archivo adjunto, pero no se pudo cargar.";
                                    }
                                    //Comprobar si el usuario existe.
                                    var usuarioHay = conexionDB.Usuario.Where(w => w.Correo == correo).ToList();
                                    if (usuarioHay.Count != 0 && usuarioHay[0].id_Perfil == 5 && usuarioHay[0].Borrado != true)
                                    {
                                        Console.WriteLine("Creando ticket de " + correo);
                                        var usuario2 = conexionDB.Usuario.Where(w => w.Correo == correo).FirstOrDefault();
                                        Ticket ticket = new Ticket();
                                        ticket.id_Usuarios = usuarioHay[0].id_Usuarios;
                                        ticket.Asunto = message.Subject;
                                        var Descripcion = "";
                                        if (message.HtmlBody != null)
                                        {
                                            ticket.Descripcion = message.HtmlBody;
                                            Descripcion = message.HtmlBody;
                                        }
                                        else
                                        {
                                            ticket.Descripcion = message.TextBody;
                                            Descripcion = message.TextBody;
                                        }
                                        ticket.id_EstadoTicket = 1;
                                        ticket.FechaCreacion = DateTime.Now.AddHours(-2);
                                        ticket.Visto = false;
                                        ticket.id_tipoTicket = 1;
                                        ticket.id_Prioridad = 3;
                                        // Agregar a la tabla
                                        conexionDB.Ticket.Add(ticket);
                                        
                                        // Guardar cambios
                                        conexionDB.SaveChanges();
                                        var ultimoid = ticket.id_Ticket.ToString();
                                        client.Inbox.AddFlags(uid, MessageFlags.Seen, true);

                                        string urlServidor = "http://help.backspace.cl/Ticket/GestionTicket/" + ultimoid;
                                        //Enviar Correo 
                                        // Información a llenar en el correo
                                        string titulo = "Ticket #" + ultimoid + "- Portal Backspace";
                                        string nombre = usuario2.Nombre + " " + usuario2.Apellido;
                                        string id = usuario2.id_Usuarios.ToString();
                                        string empresa = usuario2.Empresa.nomEmpresa;
                                        string asunto = message.Subject;
                                        int PrioridadTicket = 3;
                                        string descripcion = Descripcion;


                                        string prioridad = conexionDB.Prioridad.Where(w => w.id_Prioridad == PrioridadTicket).Select(s => s.nomPrioridad).FirstOrDefault().ToString();
                                        string textoCorreo = System.IO.File.ReadAllText("C:\\Users\\PC\\source\\repos\\MailApp\\MailApp\\Styles\\CorreoTicket.html").Replace("[Nombre]", nombre).Replace("[id]", id).Replace("[Correo]", correo).Replace("[Empresa]", empresa).Replace("[Asunto]", asunto).Replace("[Descripcion]", descripcion).Replace("[Prioridad]", prioridad).Replace("[idTicket]", ultimoid).Replace("[URLServidor]", urlServidor);
                                        EnviarMail(textoCorreo, "soporte@backspace.cl", titulo, "", "");

                                        //Enviar Correo al Cliente
                                        string titulo2 = "Ticket #" + ultimoid + "- Portal Backspace";
                                        string textoCorreoCliente = System.IO.File.ReadAllText("C:\\Users\\PC\\source\\repos\\MailApp\\MailApp\\Styles\\CorreoTicketCreadoCliente.html").Replace("[Asunto]", asunto).Replace("[URLServidor]", urlServidor);
                                        EnviarMail(textoCorreoCliente, ticket.Usuario.Correo, titulo2, correo, "");

                                    }
                                    else
                                    {
                                        System.Console.WriteLine("El usuario " + correo + " no existe en el sistema o es administrador, no se pudo crear el ticket.");
                                    }
                                }
                            }
                        }
                        client.Inbox.AddFlags(uid, MessageFlags.Seen, true);
                        System.Console.WriteLine("El correo de " + message.From.ToString() + " se marcó como visto");
                    }
                    System.Console.WriteLine("Busqueda finalizada");
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("ERROR! - " + ex.Message);
            }
        }

        public static List<Message> FetchAllMessages(string username, string password)
        {
            // The client disconnects from the server when being disposed
            using (Pop3Client client = new Pop3Client())
            {
                // Connect to the server

                client.Connect("pop.gmail.com", 995, true);

                // Authenticate ourselves towards the server
                client.Authenticate(username, password);

                // Get the number of messages in the inbox
                int messageCount = client.GetMessageCount();
                var asd = client.GetMessageUids().Count;

                // We want to download all messages
                //var uids = client.GetMessageUids();
                List<Message> allMessages = new List<Message>(messageCount);

                //// Messages are numbered in the interval: [1, messageCount]
                //// Ergo: message numbers are 1-based.
                //// Most servers give the latest message the highest number
                for (int i = messageCount; i > 0; i--)
                {

                    //if (client.GetMessage(i).Headers.From.ToString().Contains("jorge")) {
                        allMessages.Add(client.GetMessage(i));
                    Console.WriteLine("Leyendo Mensaje " + i.ToString());
                    var hola = client.GetMessage(i);
                    //}

                }
                //MessagePart xml = allMessages[0].FindFirstMessagePartWithMediaType("text/xml");
                //// Now return the fetched messages
                //return allMessages;
                
                return allMessages;
            }
        }

        private static string ObtenerCorreo(string From) {
            string correo = From;
            if (From.Contains("<") || From.Contains(">"))
            {
                var splitFrom = From.Split('<');
                var splitFromFrom = splitFrom[1].Split('>');
                correo = splitFromFrom[0];
            }


            return correo;
        }

        public static void EnviarMail(string texto, string txtCorreo, string titulo, string ConCopia, string Archivo)
        {
            using (MailMessage mail = new MailMessage())

            {
                mail.From = new MailAddress("soporte@backspace.cl");
                mail.To.Add(txtCorreo);
                mail.Subject = titulo;
                if (txtCorreo == "soporte@backspace.cl")
                {
                    MailAddress copy = new MailAddress("pablo.castro@backspace.cl");
                    mail.CC.Add(copy);

                    MailAddress copy2 = new MailAddress("jorge.cortez@backspace.cl");
                    mail.CC.Add(copy2);

                    MailAddress copy3 = new MailAddress("maximiliano.campos@backspace.cl");
                    mail.CC.Add(copy3);
                }
                if (ConCopia != "")
                {
                    var ConCC = ConCopia.Split(';');
                    for (int i = 0; i < ConCC.Length; i++)
                    {
                        if (ConCC[i] != "")
                        {
                            MailAddress cc = new MailAddress(ConCC[i]);
                            mail.CC.Add(cc);
                        }

                    }

                }
                mail.Body = texto;
                mail.IsBodyHtml = true;

                //Enviar Archivo
                if (Archivo != "" || Archivo != null)
                {
                    var Archivos = Archivo.Split(';');
                    for (int i = 0; i < Archivos.Length; i++)
                    {
                        if (Archivos[i] != "")
                        {
                            mail.Attachments.Add(new Attachment(Archivos[i]));
                        }
                    }
                }




                using (SmtpClient client = new SmtpClient())
                {
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["Usuario"], ConfigurationManager.AppSettings["Password"]);
                    client.Host = "smtp.gmail.com";
                    client.Port = 587;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.Send(mail);
                }

            }

        }

        public static string GetFinalRedirect(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            int maxRedirCount = 8;  // prevent infinite loops
            string newUrl = url;
            //do
            //{
                HttpWebRequest req = null;
                HttpWebResponse resp = null;
                try
                {
                    req = (HttpWebRequest)HttpWebRequest.Create(url);
                    req.Method = "HEAD";
                    req.AllowAutoRedirect = false;
                    resp = (HttpWebResponse)req.GetResponse();
                    switch (resp.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            return newUrl;
                        case HttpStatusCode.Redirect:
                        case HttpStatusCode.MovedPermanently:
                        case HttpStatusCode.RedirectKeepVerb:
                        case HttpStatusCode.RedirectMethod:
                            newUrl = resp.Headers["Location"];
                            if (newUrl == null)
                                return url;

                            if (newUrl.IndexOf("://", System.StringComparison.Ordinal) == -1)
                            {
                                // Doesn't have a URL Schema, meaning it's a relative or absolute URL
                                Uri u = new Uri(new Uri(url), newUrl);
                                newUrl = u.ToString();
                            }
                            break;
                        default:
                            return newUrl;
                    }
                    url = newUrl;
                }
                catch (WebException)
                {
                    // Return the last known good URL
                    return newUrl;
                }
                catch (Exception ex)
                {
                    return null;
                }
                finally
                {
                    if (resp != null)
                        resp.Close();
                }
            //} while (maxRedirCount-- > 0);

            return newUrl;
        }
    }
}
