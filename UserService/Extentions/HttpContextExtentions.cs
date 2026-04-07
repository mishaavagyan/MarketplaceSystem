namespace UserService.Extentions
{
    public static class HttpContextExtentions
    {
        public static Guid GetUserId(this HttpContext context)
        {
            return (Guid)context.Items["UserId"];
        }
    }
}
