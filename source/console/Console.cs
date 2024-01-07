using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

[GlobalClass]
public partial class Console : Node2D
{
    private float _lastY;
    private float _currentY;
    private float _targetY;
    private float _lerpSpeed = 10f;
    private Color _consoleColor = Colors.SlateGray;
    // height in screen fraction
    private float _inputHeight = 0.05f;
    // left pad in pixels
    private float _inputLeftPad = 6f;
    // bottom pad in pixels
    private float _inputBottomPad = 8f;
    private Color _inputColor = Colors.DarkSlateGray;
    [Export] public Font inputFont;
    private float _inputCursorWidth = 4f;
    private Color _inputCursorColor = Colors.WhiteSmoke;
    private float _inputCursorBlinkTime = 0.5f;
    private float _inputCursorBlinkTimer;
    
    private bool _devConsoleKeyPressedLastFrame;
    private string _text = "";
    private readonly List<(string text, HistoryType type)> _history;
    private Color _historyNormalColor = Colors.White;
    private Color _historyWarningColor = Colors.Orange;
    private Color _historyErrorColor = Colors.IndianRed;
    [Export] public Font historyFont;
    [Export] public int maxHistoryLength = 32;

    private readonly Variant[] _consoleVariant;

    public readonly List<CommandData> commands = new()
    {
        new CommandData("quit", Callable.From<Console>(QuitCmd)),
        new CommandData("get-gravity", Callable.From<Console>(GetGravityCmd)),
        new CommandData("set-gravity", Callable.From<Console, string>(SetGravityCmd)),
    };

    public static void QuitCmd(Console console) => console.GetTree().Quit();

    public static void GetGravityCmd(Console console)
    {
        var constants = console.GetNode("/root/Constants");
        var result = constants.Get("gravity").AsSingle();
        console.PushNormal($"Gravity: {result}");
    }

    public static void SetGravityCmd(Console console, string amount)
    {
        if (!float.TryParse(amount, out var newGravity))
        {
            console.PushError($"Expected float, got '{amount}' instead.");
            return;
        }

        var constants = console.GetNode("/root/Constants");
        constants.Set("gravity", Variant.From(newGravity));
        console.PushNormal($"Gravity: {newGravity}");
    }

    public Console()
    {
        _consoleVariant = new[] { Variant.From(this) };
        _history = new List<(string, HistoryType)>(maxHistoryLength);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventKey iek || _targetY == 0 || !iek.Pressed) return;
        
        switch (iek.Keycode)
        {
            case Key.Backspace when _text.Length > 0:
                _text = _text[..^1];
                QueueRedraw();
                break;
            case Key.Enter when _text.Length > 0:
                ExecuteCommand(_text);
                _text = "";
                QueueRedraw();
                break;
            case Key.Space when _text.Length > 0:
                _text += ' ';
                QueueRedraw();
                break;
            case Key.Minus:
                _text += '-';
                QueueRedraw();
                break;
            case >= Key.A and <= Key.Z
                or >= Key.Key0 and <= Key.Key9
                or >= Key.Kp0 and <= Key.Kp1
                or Key.Period:
                var text = OS.GetKeycodeString(iek.KeyLabel);
                _text += iek.ShiftPressed ? text : text.ToLower();
                QueueRedraw();
                break;
        }
    }

    public void PushLine(string text, HistoryType type)
    {
        // TODO: use a linked list instead? the insert optimization is probably not worth it tbh
        // remove excess from the tail
        if (_history.Count >= maxHistoryLength)
            _history.RemoveRange(maxHistoryLength - 2, 1 + _history.Count - maxHistoryLength);
        // and insert at the head
        _history.Insert(0, (text, type));
    }

    public void PushNormal(string text) => PushLine(text, HistoryType.Normal);
    public void PushWarning(string text) => PushLine(text, HistoryType.Warning);
    public void PushError(string text) => PushLine(text, HistoryType.Error);

    public void ExecuteCommand(string text)
    {
        PushNormal($"] {text}");
        var commandParts = text.Split();
        var commandName = commandParts[0].ToLower();
        CommandData firstApplicable = null;
        foreach (var command in commands.Where(c => c.name == commandName))
        {
            if (firstApplicable == null)
                firstApplicable = command;
            else
            {
                PushWarning($"There are multiple commands named '{commandName}'.");
                break;
            }
        }

        if (firstApplicable == null)
        {
            PushError($"Unknown command '{commandName}'.");
            return;
        }

        // TODO: support other argument types? Or should the callee just always handle parsing
        var commandArguments = commandParts.Length > 1
            ? _consoleVariant.Concat(commandParts[1..].Select(c => Variant.From(c))).ToArray()
            : _consoleVariant;
        
        // TODO: if commandArguments length doesnt match arguments expected by the function, warn
        firstApplicable.function.Call(commandArguments);
    }

    public override void _Process(double delta)
    {
        var pressedThisFrame = Input.IsKeyPressed(Key.Quoteleft);
        if (pressedThisFrame && !_devConsoleKeyPressedLastFrame)
            _targetY = _targetY > 0 ? 0f : 0.5f;

        _devConsoleKeyPressedLastFrame = pressedThisFrame;

        _currentY = Mathf.Lerp(_currentY, _targetY, (float)delta * _lerpSpeed);
        if (Mathf.IsEqualApprox(_currentY, _targetY))
            _currentY = _targetY;

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (_currentY != _lastY)
            QueueRedraw();

        _lastY = _currentY;

        if (_targetY > 0)
        {
            var previousTimer = _inputCursorBlinkTimer;
            _inputCursorBlinkTimer -= (float)delta;
            if (_inputCursorBlinkTimer <= 0f)
            {
                // the *2 avoids having to state flip a boolean or something, instead we can just check 
                // if (_inputCursorBlinkTimer > _inputCursorBlinkTime) to see if we should be visible
                _inputCursorBlinkTimer = _inputCursorBlinkTime * 2f;
                QueueRedraw();
            }
            else if (_inputCursorBlinkTimer <= _inputCursorBlinkTime && previousTimer > _inputCursorBlinkTimer)
            {
                // only queue the redraw if we've crossed the blink threshold
                QueueRedraw();
            }
        }
    }

    public override void _Draw()
    {
        var isOnScreen = _currentY > 0;

        if (!isOnScreen) return;
        
        var consoleSize = GetViewportRect().Size;
        var inputSize = consoleSize;
            
        // draw console background
        consoleSize.Y *= _currentY;
        var consoleRect = new Rect2(Vector2.Zero, consoleSize);
        DrawRect(consoleRect, _consoleColor);

        // draw input background
        inputSize.Y *= _inputHeight;
        var inputRect = new Rect2(new Vector2(0f, consoleSize.Y - inputSize.Y), inputSize);
        DrawRect(inputRect, _inputColor);

        if (inputFont != null)
        {
            var textPos = new Vector2(_inputLeftPad, consoleSize.Y - _inputBottomPad);
            Vector2 textSize;
            if (_text.Length > 0)
            {
                // draw input text
                DrawString(inputFont, textPos, _text);
                textSize = inputFont.GetStringSize(_text);
            }
            else
            {
                // even if there is no text, we still want to draw the input cursor
                textSize = inputFont.GetStringSize("|");
                // textSize will likely have a non-zero width, which wont look correct when the cursor is rendered
                // without any actual _text with it, so we get rid of the width
                textSize.X = 0f;
            }

            // draw input cursor
            if (_inputCursorBlinkTimer > _inputCursorBlinkTime)
            {
                var cursorRect = new Rect2(textPos.X + textSize.X, textPos.Y - textSize.Y, _inputCursorWidth,
                    textSize.Y);
                DrawRect(cursorRect, _inputCursorColor, false);
            }
        }

        // draw history buffer
        if (historyFont != null && _history.Count > 0)
        {
            var linePos = new Vector2(_inputLeftPad, consoleSize.Y - inputSize.Y - _inputBottomPad);
            foreach (var (line, type) in _history)
            {
                var lineColor = type switch
                {
                    HistoryType.Normal => _historyNormalColor,
                    HistoryType.Warning => _historyWarningColor,
                    HistoryType.Error => _historyErrorColor,
                    _ => _historyNormalColor
                };
                
                DrawString(historyFont, linePos, line, modulate: lineColor);
                var textSize = historyFont.GetStringSize(line);
                linePos.Y -= textSize.Y;
                if (linePos.Y < 0f)
                    break;
            }
        }

        // draw autocomplete suggestion box
    }
}

public enum HistoryType
{
    Normal,
    Warning,
    Error
}
