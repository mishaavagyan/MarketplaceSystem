namespace PhoneService.Extentions
{
    public static class HttpContextExtention
    {
        public static Guid GetUserId(this HttpContext context)
        {
            return (Guid)context.Items["UserId"];
        }
    }
}
