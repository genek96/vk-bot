using Serilog;
using VkBot.Storing;
using VkBot.Storing.Models;
using VkLongPolling;
using VkLongPolling.Configuration;
using VkLongPolling.EventHandlers;
using VkLongPolling.Models;

namespace VkBot.Configuration;

public class LongPollerConfigurator
{
    public LongPollerConfigurator(IUserStateStorage userStateStorage, ClientSettings settings)
    {
        _userStateStorage = userStateStorage;
        _settings = settings;
    }

    public LongPoller Configure()
    {
        return new LongPoller(_settings, ConfigureBuilder, Log.Logger);
    }

    private EventHandlersChainBuilder ConfigureBuilder(EventHandlersChainBuilder b)
    {
        return b.AddNewMessageHandler(async e =>
            {
                var state = await _userStateStorage.GetUserStateAsync(e.Message.FromId);
                return state == UserState.Initial;
            }, async (e, responseSender) =>
            {
                await _userStateStorage.UpdateUserAsync(e.Message.FromId, u =>
                {
                    u.CurrentState = UserState.Idle;
                    u.Coins = 100;
                    u.WorkDuration = 10;
                    u.Energy = MaxEnergy;
                });

                var user = await _userStateStorage.GetUserAsync(e.Message.FromId);
                await responseSender.SendMessageAsync(
                    e.Message.FromId,
                    "Приветствуем тебя в игре \"Фрилансер\"! С чего планируешь начать?\n" + GetStatusText(user!),
                    _staticKeyboards[UserState.Idle]
                );
            })
            .AddNewMessageHandler(async e =>
            {
                var state = await _userStateStorage.GetUserStateAsync(e.Message.FromId);
                return state == UserState.Idle && e.Message.Payload?.Text == "beginWork";
            }, async (e, responder) =>
            {
                var user = (await _userStateStorage.GetUserAsync(e.Message.FromId))!;
                if (user.Energy == 0)
                {
                    await responder.SendMessageAsync(
                        e.Message.FromId,
                        "Энергия уже на нуле. Кажется пора поспать",
                        _staticKeyboards[UserState.Idle]
                    );
                    return;
                }

                await _userStateStorage.UpdateUserAsync(e.Message.FromId, u =>
                {
                    u.CurrentState = UserState.Working;
                    u.LastActivityTime = DateTime.UtcNow;
                    u.Energy -= 1;
                });

                await responder.SendMessageAsync(
                    e.Message.FromId,
                    $"Работа продлится {user.WorkDuration} минут",
                    _staticKeyboards[UserState.Working]
                );
            })
            .AddCallbackHandler(async e =>
            {
                var state = await _userStateStorage.GetUserStateAsync(e.UserId);
                return state == UserState.Working && e.Payload.Text == "checkStatus";
            }, async (e, responder) =>
            {
                var user = (await _userStateStorage.GetUserAsync(e.UserId))!;
                var diff = DateTime.UtcNow - user.LastActivityTime;
                if (diff.TotalMinutes >= user.WorkDuration)
                {
                    var hasCorrections = Random.Shared.Next(1, 6) == 1 && user.Energy > 0;
                    if (hasCorrections)
                    {
                        await _userStateStorage.UpdateUserAsync(e.UserId, u =>
                        {
                            u.CurrentState = UserState.Working;
                            u.LastActivityTime = DateTime.UtcNow;
                            u.Energy -= 1;
                        });
                        await responder.SendMessageEventAnswerAsync(
                            e.UserId,
                            e.EventId,
                            new SnackbarAnswer("Упс, заказчик урод - прислал правки. Теперь всё переделывать заново!")
                        );
                    }
                    else
                    {
                        var reward = 50 * Random.Shared.Next(1, 3);
                        await _userStateStorage.UpdateUserAsync(e.UserId, u =>
                        {
                            u.CurrentState = UserState.JustFinishedWok;
                            u.LastActivityTime = DateTime.UtcNow;
                            u.Coins += reward;
                        });
                        await responder.SendMessageEventAnswerAsync(
                            e.UserId,
                            e.EventId,
                            new SnackbarAnswer($"Готово! Заказчик перечслил {reward} рублей.")
                        );

                        await responder.SendMessageAsync(
                            e.UserId,
                            "Рассказать об успехе за дополнительных 50р?",
                            _staticKeyboards[UserState.JustFinishedWok]
                        );
                    }

                    return;
                }

                var remain = Math.Floor(user.WorkDuration - diff.TotalMinutes);
                await responder.SendMessageEventAnswerAsync(e.UserId, e.EventId,
                    new SnackbarAnswer($"Осталось минут: {remain}"));
            })
            .AddNewMessageHandler(async e =>
            {
                var state = await _userStateStorage.GetUserStateAsync(e.Message.FromId);
                return state == UserState.Working && e.Message.Payload?.Text == "break";
            }, async (e, responder) =>
            {
                await _userStateStorage.SetUserStateAsync(e.Message.FromId, UserState.Idle);
                await responder.SendMessageAsync(
                    e.Message.FromId,
                    "Нет работ - нет забот. Чем займёмся дальше?",
                    _staticKeyboards[UserState.Idle]);
            })
            .AddNewMessageHandler(async e =>
            {
                var state = await _userStateStorage.GetUserStateAsync(e.Message.FromId);
                return state == UserState.JustFinishedWok && e.Message.Payload?.Text == "notNow";
            }, async (e, responder) =>
            {
                await _userStateStorage.SetUserStateAsync(e.Message.FromId, UserState.Idle);
                var user = (await _userStateStorage.GetUserAsync(e.Message.FromId))!;
                await responder.SendMessageAsync(
                    e.Message.FromId,
                    "Чем займёмся дальше?\n" + GetStatusText(user),
                    _staticKeyboards[UserState.Idle]
                );
            })
            .AddCallbackHandler(async e =>
            {
                var state = await _userStateStorage.GetUserStateAsync(e.UserId);
                return state == UserState.JustFinishedWok && e.Payload.Text == "repost";
            }, async (e, responder) =>
            {
                await responder.SendMessageEventAnswerAsync(
                    e.UserId,
                    e.EventId,
                    new OpenLinkAnswer("https://www.youtube.com/watch?v=dQw4w9WgXcQ")
                );
                await _userStateStorage.UpdateUserAsync(e.UserId, u =>
                {
                    u.Coins += 50;
                    u.CurrentState = UserState.Idle;
                });
                var user = await _userStateStorage.GetUserAsync(e.UserId);
                await responder.SendMessageAsync(
                    e.UserId,
                    "Чем займёмся дальше?\n" + GetStatusText(user!),
                    _staticKeyboards[UserState.Idle]
                );
            })
            .AddNewMessageHandler(async e =>
            {
                var state = await _userStateStorage.GetUserStateAsync(e.Message.FromId);
                return state == UserState.Idle && e.Message.Payload?.Text == "goToShop";
            }, async (e, responder) =>
            {
                var user = await _userStateStorage.GetUserAsync(e.Message.FromId);
                await _userStateStorage.SetUserStateAsync(e.Message.FromId, UserState.InShop);
                await responder.SendMessageAsync(e.Message.FromId, "Что хотите приобрести?", GetShopKeyboard(GetPcPrice(user!)));
            })
            .AddNewMessageHandler(async e =>
            {
                var state = await _userStateStorage.GetUserStateAsync(e.Message.FromId);
                return state == UserState.InShop && e.Message.Payload?.Text is "newPc" or "energy";
            }, async (e, responder) =>
            {
                var user = await _userStateStorage.GetUserAsync(e.Message.FromId);
                var pcPrice = GetPcPrice(user!);
                if (e.Message.Payload?.Text == "newPc")
                {
                    if (user.Coins < pcPrice)
                    {
                        await responder.SendMessageAsync(
                            e.Message.FromId,
                            "К сожалению вам не хватает денег",
                            GetShopKeyboard(pcPrice)
                        );
                        return;
                    }

                    if (user.WorkDuration <= 2)
                    {
                        await responder.SendMessageAsync(
                            e.Message.FromId,
                            "Компов круче твоего пока не привезли. Может что нибудь другое?",
                            GetShopKeyboard(pcPrice)
                        );
                        return;
                    }

                    await _userStateStorage.UpdateUserAsync(e.Message.FromId, u =>
                    {
                        u.Coins -= pcPrice;
                        u.WorkDuration -= 2;
                    });
                }
                else
                {
                    if (user.Coins < EnergyPrice)
                    {
                        await responder.SendMessageAsync(
                            e.Message.FromId,
                            "К сожалению вам не хватает денег",
                            GetShopKeyboard(pcPrice)
                        );
                        return;
                    }

                    if (user.Energy >= MaxEnergy)
                    {
                        await responder.SendMessageAsync(
                            e.Message.FromId,
                            "Ты и так полон энергии. Присмотри что-нибдь ещё.",
                            GetShopKeyboard(pcPrice)
                        );
                        return;
                    }

                    await _userStateStorage.UpdateUserAsync(e.Message.FromId, u =>
                    {
                        u.Coins -= EnergyPrice;
                        u.Energy += 2;
                    });
                }

                await responder.SendMessageAsync(
                    e.Message.FromId,
                    "Отличная покупка! Что-нибудь ещё?",
                    GetShopKeyboard(pcPrice)
                );
            })
            .AddNewMessageHandler(async e =>
            {
                var state = await _userStateStorage.GetUserStateAsync(e.Message.FromId);
                return state == UserState.InShop && e.Message.Payload?.Text == "back";
            }, async (e, responder) =>
            {
                await _userStateStorage.SetUserStateAsync(e.Message.FromId, UserState.Idle);
                var user = await _userStateStorage.GetUserAsync(e.Message.FromId);
                await responder.SendMessageAsync(
                    e.Message.FromId,
                    GetStatusText(user!) + "\nЧем займёшься дальше?",
                    _staticKeyboards[UserState.Idle]
                );
            })
            .AddNewMessageHandler(async e =>
            {
                var state = await _userStateStorage.GetUserStateAsync(e.Message.FromId);
                return state == UserState.Idle && e.Message.Payload?.Text == "goToSleep";
            }, async (e, responder) =>
            {
                var user = await _userStateStorage.GetUserAsync(e.Message.FromId);
                if (user!.Energy >= MaxEnergy)
                {
                    await responder.SendMessageAsync(
                        user.UserId,
                        "Ты и так полон сил, не время спать!",
                        _staticKeyboards[UserState.Idle]
                    );
                    return;
                }

                await _userStateStorage.UpdateUserAsync(user.UserId, u =>
                {
                    u.LastActivityTime = DateTime.UtcNow;
                    u.CurrentState = UserState.Sleeping;
                });

                await responder.SendMessageAsync(
                    user.UserId,
                    "Добрых снов!",
                    _staticKeyboards[UserState.Sleeping]
                );
            })
            .AddCallbackHandler(async e =>
            {
                var state = await _userStateStorage.GetUserStateAsync(e.UserId);
                return state == UserState.Sleeping && e.Payload.Text == "checkStatus";
            }, async (e, responder) =>
            {
                var user = await _userStateStorage.GetUserAsync(e.UserId);
                var diff = DateTime.UtcNow - user!.LastActivityTime;
                if (diff.TotalMinutes < 60)
                {
                    var remain = Math.Floor(60 - diff.TotalMinutes);
                    await responder.SendMessageEventAnswerAsync(
                        e.UserId,
                        e.EventId,
                        new SnackbarAnswer($"Осталось спать ещё {remain}")
                    );
                    return;
                }

                await _userStateStorage.UpdateUserAsync(user.UserId, u =>
                {
                    u.Energy = MaxEnergy;
                    u.CurrentState = UserState.Idle;
                });

                await responder.SendMessageEventAnswerAsync(
                    e.UserId,
                    e.EventId,
                    new SnackbarAnswer("С добрым утром! Энергия полностью восполнена.")
                );

                await responder.SendMessageAsync(
                    user.UserId,
                    "Чем займёшься?\n" + GetStatusText(user),
                    _staticKeyboards[UserState.Idle]
                );
            })
            .AddNewMessageHandler(async e =>
            {
                var state = await _userStateStorage.GetUserStateAsync(e.Message.FromId);
                return state == UserState.Sleeping && e.Message.Payload?.Text == "break";
            }, async (e, responder) =>
            {
                var user = await _userStateStorage.GetUserAsync(e.Message.FromId);
                await _userStateStorage.SetUserStateAsync(user!.UserId, UserState.Idle);
                await responder.SendMessageAsync(
                    user.UserId,
                    "Иногда можно и поработать ночью, главное не часто. Чем займёшься?\n" + GetStatusText(user),
                    _staticKeyboards[UserState.Idle]
                );
            })
            .AddNewMessageHandler(
                e => new ValueTask<bool>(true), async (e, responder) =>
                {
                    var user = await _userStateStorage.GetUserAsync(e.Message.FromId);
                    await responder.SendMessageAsync(
                        e.Message.FromId,
                        "Команда не известна",
                        user!.CurrentState == UserState.InShop
                            ? GetShopKeyboard(GetPcPrice(user))
                            : _staticKeyboards[user.CurrentState]
                    );
                });
    }

    private static string GetStatusText(User user) => $"Всего рублей: {user.Coins}\n" +
                                                      $"Энергия: {user.Energy}\n" +
                                                      $"Время на работу: {user.WorkDuration}";

    private readonly Dictionary<UserState, Keyboard> _staticKeyboards = new()
    {
        [UserState.Idle] = new KeyboardBuilder()
            .AddTextButton("Начать работать над заказом", new Payload("beginWork"))
            .AddNewButtonsLine()
            .AddTextButton("Отправиться в Магазин", new Payload("goToShop"))
            .AddNewButtonsLine()
            .AddTextButton("Лечь спать", new Payload("goToSleep")),
        [UserState.Sleeping] = new KeyboardBuilder()
            .AddCallbackButton("Когда уже будильник?", new Payload("checkStatus"))
            .AddNewButtonsLine()
            .AddTextButton("Прервать сон", new Payload("break"), ButtonColor.Negative),
        [UserState.Working] = new KeyboardBuilder()
            .AddCallbackButton("Уже готово?", new Payload("checkStatus"))
            .AddNewButtonsLine()
            .AddTextButton("Бросить заказ", new Payload("break"), ButtonColor.Negative),
        [UserState.JustFinishedWok] = new KeyboardBuilder()
            .AddCallbackButton("Конечно!", new Payload("repost"), ButtonColor.Positive)
            .AddTextButton("В другой раз", new Payload("notNow"))
    };

    private static Keyboard GetShopKeyboard(int pcPrice) => new KeyboardBuilder()
        .AddTextButton($"Новый комп - {pcPrice}руб. (-2 мин.)", new Payload("newPc"))
        .AddNewButtonsLine()
        .AddTextButton($"Энергетик - {EnergyPrice}руб. (+2 энергии.)", new Payload("energy"))
        .AddNewButtonsLine()
        .AddTextButton("Назад", new Payload("back"), ButtonColor.Secondary);

    private static int GetPcPrice(User user) => 100 * (100 / user.WorkDuration);

    private const int MaxEnergy = 10;
    private const int EnergyPrice = 10;

    private readonly IUserStateStorage _userStateStorage;
    private readonly ClientSettings _settings;
}