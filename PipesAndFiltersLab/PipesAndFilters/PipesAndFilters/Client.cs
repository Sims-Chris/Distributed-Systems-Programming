using System;
using System.Collections.Generic;
using System.Text;

namespace PipesAndFilters
{
    class Client
    {
        int userId = 1;
        public void RequestHello(string messageToSend)
        {
            IMessage message = new Message();

            // Add the user ID header
            message.Headers.Add("User", userId.ToString());

            // Convert the message to a byte array and then turn the byte array into a string of byte values delimited by dashes
            message.Headers.Add("RequestFormat", "Bytes");
            byte[] bytes = Encoding.ASCII.GetBytes(messageToSend);
            string requestBody = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                requestBody += bytes[i].ToString();
                if (i + 1 < bytes.Length)
                {
                    requestBody += "-";
                }
            }
            message.Body = requestBody;

            // Send the message and get the response back
            IMessage response = ServerEnvironment.SendRequest(message);

            // Get the timestamp from the response
            response.Headers.TryGetValue("Timestamp", out string timestamp);

            // Turn the delimited string of bytes to a byte array and then to an ASCII string
            string responseBody = "";
            string[] byteStrings = response.Body.Split('-');
            bytes = new byte[byteStrings.Length];
            for (int i = 0; i < byteStrings.Length; i++)
            {
                bytes[i] = byte.Parse(byteStrings[i]);
            }
            responseBody = Encoding.ASCII.GetString(bytes);

            // Output the response to the Console
            Console.WriteLine($"At {timestamp} Response was: {responseBody}");
        }
    }
}
