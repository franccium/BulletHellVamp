using Godot;
using System;

public partial class DialogueWindow : Control
{
    private TextureRect _portrait;
    private Label _name;
    private Label _text;
    private Panel _dialoguePanel;
    private string[] _pageTexts;
    private int _pageCount;

    public override void _Ready()
    {
        GatherRequirements();
        DisplayDialogue(false);
    }

    private void GatherRequirements()
    {
        _portrait = GetNode<TextureRect>("PortraitSprite");
        _name = GetNode<Label>("NameText");
        _text = GetNode<Label>("DialogueText");
        _dialoguePanel = GetNode<Panel>("DialoguePanel");
    }

    private void DisplayDialogue(bool to_display)
    {
        if (to_display)
        {
            _portrait.Visible = true;
            _name.Visible = true;
            _text.Visible = true;
        }
        else
        {
            _portrait.Visible = false;
            _name.Visible = false;
            _text.Visible = false;
        }
    }

    public override void _Process(double delta)
    {
    }

    public void CreateDialogue(string name, string text, Texture2D portrait) //todo splitting dialogue; formatting like bolding a certain part and setting a different font, colour
    {
        _name.Text = name;
        _text.Text = text;
        _portrait.Texture = portrait;
        DividePages();
        DisplayDialogue(true);
    }

    public void GatherInput()
    {
        if (Input.IsActionJustPressed("next_dialogue"))
        {
            if (_pageCount > 1)
            {
                --_pageCount;
                _text.Text = _pageTexts[_pageCount];
            }
            else
            {
                DisplayDialogue(false);
            }
        }
    }

    private void DividePages()
    {
        int charCount = _text.Text.Length;
        int textLength = charCount * (_text.GetThemeFontSize("font") / 2);
        _pageCount = 1 + textLength / (int)(_dialoguePanel.GetRect().Size.X - _portrait.GetRect().Size.X);
        _pageTexts = new string[_pageCount];
        _pageTexts[0] = _text.Text;
    }
}
