using System;
using System.Collections.Generic;
using System.Text;

namespace PipesAndFilters
{
    public class HelloWorldEndpoint
    {
        public IMessage Execute(IMessage message)
        {
            Message response = new Message();
            response.Body = $"Hello {ServerEnvironment.CurrentUser.Name}! You sent the message: {message.Body}";
            response.Headers.Add("ResponseFormat", message.Headers["RequestFormat"]);
            return response;
        }
    }
}
