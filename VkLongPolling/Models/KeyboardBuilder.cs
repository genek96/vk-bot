namespace VkLongPolling.Models;

public class KeyboardBuilder
{
    public KeyboardBuilder(bool oneTime = true, bool inline = false)
    {
        _oneTime = oneTime;
        _inline = inline;
        _buttons = new List<List<Button>>();
        _buttons.Add(new List<Button>());
    }

    public KeyboardBuilder AddTextButton(string label, Payload? payload = null, ButtonColor color = ButtonColor.Primary)
    {
        _buttons.Last().Add(new Button(new TextButtonAction(label, payload), color));
        return this;
    }

    public KeyboardBuilder AddCallbackButton(string label, Payload? payload = null, ButtonColor color = ButtonColor.Primary)
    {
        _buttons.Last().Add(new Button(new CallbackButtonAction(label, payload), color));
        return this;
    }

    public KeyboardBuilder AddNewButtonsLine()
    {
        _buttons.Add(new List<Button>());
        return this;
    }

    public static implicit operator Keyboard(KeyboardBuilder builder)
    {
        return new Keyboard(
            builder._oneTime,
            builder._buttons.Select(x => x.ToArray()).ToArray(),
            builder._inline
        );
    }

    private List<List<Button>> _buttons;
    private readonly bool _inline;
    private readonly bool _oneTime;
}