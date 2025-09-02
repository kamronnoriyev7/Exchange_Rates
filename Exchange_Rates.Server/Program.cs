using System;
using System.Collections.Generic;
using System.Globalization; // CultureInfo uchun
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

class Program
{
    private static readonly string botToken = "7948406890:AAEjfZGka48Fhja_yl7FALhb-DZriXf7-h0";
    private static readonly string apiUrl = "https://cbu.uz/uz/arkhiv-kursov-valyut/json/";
    private static Dictionary<long, string> userLanguages = new Dictionary<long, string>(); 

    static async Task Main()
    {
        var botClient = new TelegramBotClient(botToken);
        botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync);
        Console.WriteLine("Bot ishga tushdi...");
        Console.ReadLine();
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
    {
        if (update.Message is not { Text: { } messageText }) return;

        long chatId = update.Message.Chat.Id;

        if (messageText == "/start")
        {
            await bot.SendTextMessageAsync(chatId, "🇺🇿 Assalomu alaykum! Valyuta kurslari botiga xush kelibsiz! \nTilni tanlang:", replyMarkup: LanguageButtons());
            return;
        }

        if (messageText == "🇺🇿 O'zbekcha")
        {
            userLanguages[chatId] = "uz";
            await bot.SendTextMessageAsync(chatId, "💱 Valyuta kurslarini tanlang:", replyMarkup: CurrencyButtons());
            return;
        }

        if (messageText == "🇷🇺 Русский")
        {
            userLanguages[chatId] = "ru";
            await bot.SendTextMessageAsync(chatId, "💱 Выберите валютную пару:", replyMarkup: CurrencyButtons());
            return;
        }

        if (messageText == "🇺🇸 English")
        {
            userLanguages[chatId] = "en"; 
            await bot.SendTextMessageAsync(chatId, "💱 Choose a currency pair:", replyMarkup: CurrencyButtons());
            return;
        }

        if (messageText == "🇺🇸 USD - UZS 🇺🇿") await SendCurrencyRate(bot, chatId, "USD", "UZS");
        if (messageText == "🇪🇺 EUR - UZS 🇺🇿") await SendCurrencyRate(bot, chatId, "EUR", "UZS");
        if (messageText == "🇷🇺 RUB - UZS 🇺🇿") await SendCurrencyRate(bot, chatId, "RUB", "UZS");
        if (messageText == "🇺🇸 USD - EUR 🇪🇺") await SendCurrencyRate(bot, chatId, "USD", "EUR");
        if (messageText == "🇪🇺 EUR - USD 🇺🇸") await SendCurrencyRate(bot, chatId, "EUR", "USD");
        if (messageText == "🇺🇸 USD - RUB 🇷🇺") await SendCurrencyRate(bot, chatId, "USD", "RUB");
        if (messageText == "🇷🇺 RUB - USD 🇺🇸") await SendCurrencyRate(bot, chatId, "RUB", "USD");
        if (messageText == "🇪🇺 EUR - RUB 🇷🇺") await SendCurrencyRate(bot, chatId, "EUR", "RUB");
        if (messageText == "🇷🇺 RUB - EUR 🇪🇺") await SendCurrencyRate(bot, chatId, "RUB", "EUR");
    }

    private static async Task SendCurrencyRate(ITelegramBotClient bot, long chatId, string fromCurrency, string toCurrency)
    {
        double rateFrom = await GetCurrencyRate(fromCurrency);
        double rateTo = await GetCurrencyRate(toCurrency);

        if (rateFrom == -1 || rateTo == -1)
        {
            await bot.SendTextMessageAsync(chatId, "Valyuta kursi topilmadi.");
            return;
        }

        double result = rateFrom / rateTo;

        
        string userLanguage = userLanguages.ContainsKey(chatId) ? userLanguages[chatId] : "uz"; 

       
        string today = DateTime.Now.ToString("dd MMMM yyyy", GetCultureInfo(userLanguage));

        string responseText = userLanguage switch
        {
            "uz" => $"📅 {today} holatiga ko'ra:\n💵 1 {fromCurrency} = {result:F2} {toCurrency}",
            "ru" => $"📅 По состоянию на {today}:\n💵 1 {fromCurrency} = {result:F2} {toCurrency}",
            "en" => $"📅 As of {today}:\n💵 1 {fromCurrency} = {result:F2} {toCurrency}",
            _ => $"📅 {today}:\n💵 1 {fromCurrency} = {result:F2} {toCurrency}"
        };

        await bot.SendTextMessageAsync(chatId, responseText);
    }

    private static CultureInfo GetCultureInfo(string userLanguage)
    {
        return userLanguage switch
        {
            "uz" => new CultureInfo("uz-UZ"), // O'zbekcha
            "ru" => new CultureInfo("ru-RU"), // Ruscha
            "en" => new CultureInfo("en-US"), // Inglizcha
            _ => new CultureInfo("uz-UZ") // Default: O'zbekcha
        };
    }

    private static async Task<double> GetCurrencyRate(string currencyCode)
    {
        // Agar valyuta UZS bo'lsa, 1 qaytaradi
        if (currencyCode == "UZS")
        {
            return 1;
        }

        using HttpClient client = new();
        string json = await client.GetStringAsync(apiUrl);
        JsonDocument doc = JsonDocument.Parse(json);

        foreach (var element in doc.RootElement.EnumerateArray())
        {
            if (element.GetProperty("Ccy").GetString() == currencyCode)
            {
                string rateString = element.GetProperty("Rate").GetString();
                if (double.TryParse(rateString, NumberStyles.Any, CultureInfo.InvariantCulture, out double rate))
                {
                    return rate;
                }
            }
        }
        return -1; // Ma'lumot topilmadi
    }

    private static ReplyKeyboardMarkup LanguageButtons() => new ReplyKeyboardMarkup(new[]
    {
        new[] { new KeyboardButton("🇺🇿 O'zbekcha"), new KeyboardButton("🇷🇺 Русский"), new KeyboardButton("🇺🇸 English") }
    })
    { ResizeKeyboard = true };

    private static ReplyKeyboardMarkup CurrencyButtons() => new ReplyKeyboardMarkup(new[]
    {
        new[] { new KeyboardButton("🇺🇸 USD - UZS 🇺🇿") },
        new[] { new KeyboardButton("🇪🇺 EUR - UZS 🇺🇿") },
        new[] { new KeyboardButton("🇷🇺 RUB - UZS 🇺🇿") },
        new[] { new KeyboardButton("🇺🇸 USD - EUR 🇪🇺"), new KeyboardButton("🇪🇺 EUR - USD 🇺🇸") },
        new[] { new KeyboardButton("🇺🇸 USD - RUB 🇷🇺"), new KeyboardButton("🇷🇺 RUB - USD 🇺🇸") },
        new[] { new KeyboardButton("🇪🇺 EUR - RUB 🇷🇺"), new KeyboardButton("🇷🇺 RUB - EUR 🇪🇺") }
    })
    { ResizeKeyboard = true };

    private static Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken token)
    {
        Console.WriteLine($"Xatolik: {exception.Message}");
        return Task.CompletedTask;
    }
}