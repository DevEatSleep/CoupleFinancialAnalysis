namespace Shared;

/// <summary>
/// Consolidated constants shared across Backend and Frontend.
/// </summary>
public static class Constants
{
    /// <summary>
    /// JSON property names used throughout the application.
    /// </summary>
    public static class JsonProperties
    {
        public const string Questions = "questions";
        public const string Id = "id";
        public const string Text = "text";
        public const string Category = "category";
        public const string Person = "person";
    }

    /// <summary>
    /// File and directory paths.
    /// </summary>
    public static class Paths
    {
        public const string QuestionsDirectory = "questions";
        public const string DatabaseFile = "chat.db";
    }

    /// <summary>
    /// CORS and networking configuration.
    /// </summary>
    public static class Network
    {
        public const string CorsPolicyName = "AllowFrontend";
        public const string ServerUrl = "http://localhost:5000";
        
        public static readonly string[] AllowedOrigins = new[]
        {
            "http://localhost:5000",
            "http://127.0.0.1:5000",
            "http://localhost:3000",
            "http://127.0.0.1:3000"
        };
    }

    /// <summary>
    /// API routes specific to backend (Swagger, Health, etc).
    /// </summary>
    public static class Routes
    {
        public const string Health = "/health";
        public const string HealthResponse = "OK";
        public const string Swagger = "swagger";
        public const string SwaggerEndpoint = "/swagger/v1/swagger.json";
        public const string SwaggerTitle = "Couple Chat API v1";
    }

    /// <summary>
    /// Supported program languages.
    /// </summary>
    public static class Languages
    {
        public const string English = "en";
        public const string French = "fr";
        public const string Spanish = "es";

        /// <summary>Array of all supported language codes.</summary>
        public static readonly string[] Supported = { English, French, Spanish };
    }

    /// <summary>
    /// Person type identifiers.
    /// </summary>
    public static class PersonTypes
    {
        public const string Woman = "woman";
        public const string Man = "man";
        public const string Shared = "shared";

        /// <summary>Array of all person types.</summary>
        public static readonly string[] All = { Woman, Man, Shared };
    }

    /// <summary>
    /// Domestic work (TravailDomestique) constants.
    /// </summary>
    public static class Domestique
    {
        /// <summary>Hourly rate for domestic work valuation (SMIC).</summary>
        public const decimal HourlyRate = 11.88m;

        /// <summary>Conversion factor from weeks to months: 52/12.</summary>
        public const decimal WeekToMonthFactor = 52m / 12m;

        /// <summary>Allowed domestic work activities.</summary>
        public static class Activities
        {
            public const string CuisineEtMenage = "cuisine & ménage";
            public const string SoinsEnfants = "soins enfants";
            public const string Courses = "courses";
            public const string BricolagJardinage = "bricolage/jardinage";

            public static readonly string[] All = new[]
            {
                CuisineEtMenage,
                SoinsEnfants,
                Courses,
                BricolagJardinage
            };
        }

        /// <summary>Age groups for INSEE reference data.</summary>
        public static class AgeRanges
        {
            public const string Range18_24 = "18-24 ans";
            public const string Range25_34 = "25-34 ans";
            public const string Range35_49 = "35-49 ans";
            public const string Range50_64 = "50-64 ans";

            public static readonly string[] All = new[]
            {
                Range18_24,
                Range25_34,
                Range35_49,
                Range50_64
            };
        }

        /// <summary>Gender/Sex identifiers for INSEE data (French).</summary>
        public static class Sexe
        {
            public const string Femme = "femme";
            public const string Homme = "homme";

            public static readonly string[] All = new[] { Femme, Homme };
        }
    }

    /// <summary>
    /// Question IDs and question configuration.
    /// </summary>
    public static class QuestionIds
    {
        // Personal questions
        public const int WomanFirstName = 1;
        public const int WomanAge = 2;
        public const int WomanSalary = 3;
        public const int ManFirstName = 4;
        public const int ManAge = 5;
        public const int ManSalary = 6;

        // Expense questions
        public const int ExpenseLabel = 7;
        public const int ExpenseAmount = 8;
        public const int ExpensePaidBy = 9;

        // Domestic work questions (woman)
        public const int WomanCuisineEtMenage = 10;
        public const int WomanSoinsEnfants = 11;
        public const int WomanCourses = 12;
        public const int WomanBricolagJardinage = 13;

        // Domestic work questions (man)
        public const int ManCuisineEtMenage = 14;
        public const int ManSoinsEnfants = 15;
        public const int ManCourses = 16;
        public const int ManBricolagJardinage = 17;

        /// <summary>Base question ID for woman's domestic work questions.</summary>
        public const int WomanDomestiqueBase = 10;

        /// <summary>Base question ID for man's domestic work questions.</summary>
        public const int ManDomestiqueBase = 14;
    }

    /// <summary>
    /// Question categories.
    /// </summary>
    public static class QuestionCategories
    {
        public const string Personal = "personal";
        public const string Financial = "financial";
        public const string Expenses = "expenses";
        public const string TravailDomestique = "travail_domestique";
    }

    /// <summary>
    /// API route endpoints.
    /// </summary>
    public static class ApiEndpoints
    {
        // Bot endpoints
        public const string BotNextQuestion = "api/bot/next-question";
        public const string BotRespond = "api/bot/respond";
        public const string BotResponses = "api/bot/responses";
        public const string BotResponse = "api/bot/response";
        public const string BotExpenses = "api/bot/expenses";
        public const string BotExpense = "api/bot/expense";
        public const string BotLanguage = "api/bot/language";

        // Domestique endpoints
        public const string DomestiqueBase = "api/domestique";

        // Chat endpoints
        public const string ChatMessages = "api/chat";

        // Reference endpoints
        public const string ReferenceBase = "api/reference";
        public const string TravailDomestique = "api/reference/travail-domestique";

        // Health check
        public const string Health = "/health";
    }

    /// <summary>
    /// Timing constants for delays and timeouts.
    /// </summary>
    public static class Timings
    {
        /// <summary>Delay between chat messages in milliseconds.</summary>
        public const int ChatMessageDelayMs = 300;
    }
}
