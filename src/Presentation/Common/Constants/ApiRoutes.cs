namespace Presentation.Common.Constants;

public static class ApiRoutes
{
    public const string HttpsBaseUrl = "https://localhost:7214";
    public const string Root = "api/v{version:apiVersion}";

    public static class Todos
    {
        public const string GetAll = "todos";
        public const string GetById = "todos/{todoId}";
        public const string Create = "todos";
        public const string UpdateById = "todos/{todoId}";
        public const string DeleteAll = "todos";
        public const string DeleteById = "todos/{todoId}";
    }
}
