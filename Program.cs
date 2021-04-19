﻿// <copyright file="Program.cs"
// All rights reserved.
// </copyright>
namespace PolynomialBot
{
    using System;
    using System.Collections.Generic;
    using Arithmetics.Polynomial1;
    using ElementaryInterpreter;
    using Telegram.Bot;
    using Telegram.Bot.Types.Enums;

    internal class Program
    {
        private static TelegramBotClient botClient;
        private static SortedList<int, Executor<Polynomial>> idExecutorPairs = new SortedList<int, Executor<Polynomial>>();

        private static void Main(string[] args)
        {
            botClient = new TelegramBotClient("1602474976:AAHPEqob_wWU9fXpLGTt5avZqKv5KesPkq8");

            // подписываемся на событие "отправка сообщений"
            botClient.OnMessage += BotClientOnMessageReceived;

            botClient.OnCallbackQuery += BotClientOnCallbackQueryReceived;

            var me = botClient.GetMeAsync().Result;

            Console.WriteLine(me.FirstName);

            // "слушаем" сообщения от сервера
            botClient.StartReceiving();

            if (Console.ReadLine() == "stop")
            {
                botClient.StopReceiving();
            }
        }

        private static async void BotClientOnCallbackQueryReceived(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            string buttonText = e.CallbackQuery.Data;
            string name = $"{e.CallbackQuery.From.FirstName} {e.CallbackQuery.From.LastName}";
            Console.WriteLine($"{name} press button {buttonText}");

            await botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, $"{buttonText}");
        }

        private static async void BotClientOnMessageReceived(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var message = e.Message;
            if (message.Type != MessageType.Text || message.Text == null)
            {
                return;
            }

            string name = $"{message.From.FirstName} {message.From.LastName}";
            Console.WriteLine($"{name} send {message.Text}");

            switch (message.Text)
            {
                case "/start":
                    string startMessage =
 @"Список команд:
/start - Старт
/clear - очистить список переменных
/regulations - Вызовите, для получения правил работы с ботом
/getvars - получить список переменных";
                    await botClient.SendTextMessageAsync(message.From.Id, startMessage);
                    break;
                case "/regulations":
                    string regulationsMessage =
 @"
Правила использования:
 Бот умеет:
  вычислять выражения типа ax^n+bx^(n-1) + ... + cx + d 
  и приводить их в стандартную форму
  где a,b,c,d -целые числа, а n - натуральное.
  Присваивать значения переменным 
  и взаимодействовать с ними.
  Например, при вводе следующего сообщения 
  'nameOne := x+1
   nameTwo := x+1
   nameOne + nameTwo'
  бот ответит
   nameOne := x+1
   nameTwo := x+1
   2x+2
 Бот поддерживает следующие функции
  Diff(nameOne*nameTwo) - дифферинцирование выражения внутри скобок
  Eval(nameOne,nameTwo) - Вычисление полинома nameOne в точке nameTwo";
                    await botClient.SendTextMessageAsync(message.From.Id, regulationsMessage);
                    break;

                // case "/inline":
                //    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                //    {
                //        new[]
                //        {
                //            InlineKeyboardButton.WithUrl("Vk", "https://vk.com"),
                //            InlineKeyboardButton.WithUrl("Telegram","https://t.me/garedin")
                //        },
                //        new[]
                //        {
                //            InlineKeyboardButton.WithCallbackData("V"),
                //            InlineKeyboardButton.WithUrl("Telegram","https://t.me/garedin")
                //        }
                //    });
                //    await BotClient.SendTextMessageAsync(message.From.Id, "Choose the option", replyMarkup: inlineKeyboard);
                //    break;
                // case "/keyboard":
                //    var replayKeyboard = new ReplyKeyboardMarkup(new[]
                //    {
                //        new[]
                //        {
                //            new KeyboardButton("hey"),
                //            new KeyboardButton("key")
                //        },
                //        new[]
                //        {
                //             new KeyboardButton("contact") { RequestContact = true},
                //             new KeyboardButton("location") { RequestLocation = true}
                //        }
                //    });
                //    await BotClient.SendTextMessageAsync(message.Chat.Id, "message", replyMarkup: replayKeyboard);
                //    break;
                case "/clear":
                    if (!idExecutorPairs.ContainsKey(message.From.Id))
                    {
                        idExecutorPairs.Add(message.From.Id, new Executor<Polynomial>());
                    }

                    idExecutorPairs.Remove(message.From.Id);
                    await botClient.SendTextMessageAsync(message.From.Id, "Очищено");
                    break;
                case "/getvars":
                    if (!idExecutorPairs.ContainsKey(message.From.Id))
                    {
                        idExecutorPairs.Add(message.From.Id, new Executor<Polynomial>());
                    }

                    string vars = idExecutorPairs[message.From.Id].GetVars();
                    if (vars == string.Empty || vars == null)
                    {
                        vars = "Упс, никаких переменных нет";
                    }

                    await botClient.SendTextMessageAsync(message.From.Id, vars);
                    break;

                // case "/generede":
                //    var replayGeneradKeyboard = new InlineKeyboardMarkup(new[]
                //    {
                //        new[]
                //        {
                //             InlineKeyboardButton.WithUrl("Password","https://castlots.org/generator-nikov-online/"),
                //             InlineKeyboardButton.WithUrl("NickName","https://castlots.org/generator-nikov-online/"),
                //        },
                //        new[]
                //        {
                //             InlineKeyboardButton.WithCallbackData("Password (here)", "x^2+2x+1"),
                //             InlineKeyboardButton.WithCallbackData("NickName (here)", "x+1"),
                //        }
                //    });
                //    await BotClient.SendTextMessageAsync(message.Chat.Id, "What U need to generade?", replyMarkup: replayGeneradKeyboard);
                //    break;
                default:
                    if (!idExecutorPairs.ContainsKey(message.From.Id))
                    {
                        idExecutorPairs.Add(message.From.Id, new Executor<Polynomial>());
                    }

                    string answer = idExecutorPairs[message.From.Id].Launch(message.Text);
                    await botClient.SendTextMessageAsync(message.From.Id, answer);
                    break;
            }
        }
    }
}
