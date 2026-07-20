using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriVision.Application.Interfaces
{
    public interface IMqttMessageHandler
    {
        Task HandleMessageAsync(string topic, byte[] payload);
    }
}
