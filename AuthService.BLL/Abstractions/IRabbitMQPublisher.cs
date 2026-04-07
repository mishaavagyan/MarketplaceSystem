using AuthService.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.BLL.Abstractions
{
    public interface IRabbitMQPublisher
    {
        public Task PublishUserCreated(UserCreatedEvent userCreated);
    }
}
