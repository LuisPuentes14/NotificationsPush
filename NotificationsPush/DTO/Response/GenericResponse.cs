using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NotificationsPush.DTO.Response
{
    public class GenericResponse<T> where T : class
    {
        public string message { get; }
        public bool status { get; }
        public T value { get; }
        public List<T> values { get; }

        public GenericResponse(bool status, string message, T value = null, List<T> values = null)
        {

            this.message = message;
            this.value = value;
            this.values = values;
            this.status = status;
        }
    }
}
