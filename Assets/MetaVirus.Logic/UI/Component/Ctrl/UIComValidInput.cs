using System.Text.RegularExpressions;
using FairyGUI;

namespace MetaVirus.Logic.UI.Component.Ctrl
{
    /**
         * 成功返回null，失败返回提示信息
         */
    public delegate string InputValidator(string input);

    /// <summary>
    /// 适用于FairyGUI中的ValidInput组件
    /// Common/CommonUI/Input/*
    /// </summary>
    public class UIComValidInput
    {
        private readonly GComponent _validInputCom;

        private const string EmailPattern =
            @"^[a-zA-Z0-9_+&*-]+(?:\.[a-zA-Z0-9_+&*-]+)*@(?:[a-zA-Z0-9-]+\.)+[a-zA-Z]{2,7}$";


        public UIComValidInput(GComponent validInput)
        {
            _validInputCom = validInput;
        }

        private string EmailValidator(string input)
        {
            var regMail = new Regex(EmailPattern);
            var match = regMail.Match(input);
            if (match.Success)
            {
                return null;
            }
            else
            {
                return input.Length > 0 ? "Incorrect email address" : "Please input email";
            }
        }

        public string GetInputEmail()
        {
            return GetInputText(EmailValidator);
        }

        public string GetInputText(InputValidator validator)
        {
            var input = _validInputCom.GetChild("text").asTextInput;
            var msg = _validInputCom.GetChild("message").asTextField;
            var ctl = _validInputCom.GetController("valid");
            var txt = input.text;
            var valid = validator?.Invoke(txt);
            if (valid == null)
            {
                ctl.SetSelectedIndex(0);
                msg.text = "";
                return txt;
            }
            else
            {
                ctl.SetSelectedIndex(1);
                msg.text = valid;
                return null;
            }
        }
    }
}