using System.Collections.Generic;
using System.Linq;
using DiceRollerPro;
using DiceRollerPro.Models;
using DiceRollerPro.Parser;
using UnityEngine;
using UnityEngine.UI;

namespace BardoUi.Chat
{
    public class Chat : MonoBehaviour
    {
        public InputField inputField;
        public Transform messageParent;
        public GameObject messagePrefab;

        private readonly List<string> _messages = new List<string>();
        private int _messageIndex = -1;
        private readonly Parser _mParser = new Parser();
        private readonly System.Random _mRandom = new System.Random();

        private GameMaster _gm;

        public static Chat instance;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            _gm = GameMaster.instance;

            foreach (Transform child in messageParent) Destroy(child.gameObject);
        }

        public void Update()
        {
            if (GameMaster.instance.currentSelectedGameObject != inputField.gameObject) return;

            if (Input.GetKeyDown(KeyCode.Return))
            {
                inputField.ActivateInputField();
                inputField.Select();

                var input = inputField.text;
                _messages.Add(input);
                _messageIndex = -1;
                inputField.text = "";

                if (input.StartsWith("/roll "))
                {
                    var rollInput = input.Substring(6).Replace(" ", "");
                    BaseRoll roll;
                    try
                    {
                        roll = _mParser.Parse(rollInput);
                    }
                    catch (SyntaxException)
                    {
                        NewMessage("Error", "red", "Check syntax", "background");
                        return;
                    }

                    var result = roll.GenerateValue(_mRandom);
                    var rollMessage = $"Rolled {rollInput}\n" +
                                      $"  {RollExplainer.Explain(result)}\n" +
                                      $"  Result: {result.Value.ToString()}";

                    AddMessage(rollMessage, "background");
                    return;
                }

                AddMessage(input);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (!_messages.Any()) return;
                _messageIndex += 1;
                _messageIndex = Mathf.Clamp(_messageIndex, 0, _messages.Count - 1);
                inputField.text = _messages[_messages.Count - 1 - _messageIndex];
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (!_messages.Any()) return;
                _messageIndex -= 1;
                _messageIndex = Mathf.Clamp(_messageIndex, 0, _messages.Count - 1);
                inputField.text = _messages[_messages.Count - 1 - _messageIndex];
            }
        }

        private void AddMessage(string message, string extras = "")
        {
            var playerText = Campaign.playerIsMaster ? "Master" : Campaign.playerName;
            _gm.RegisterAction(new Action
            {
                name = GameMaster.ActionNames.AddMessage,
                strings = new List<string> {playerText, "blue", message, extras}
            });
            NewMessage(playerText, "green", message, extras);
        }

        public void NewMessage(string sender, string color, string message, string extras = "")
        {
            var messageGo = Instantiate(messagePrefab, messageParent);
            messageGo.GetComponent<InputField>().text = $"<color={color}>{sender}:</color> {message}";
            messageGo.GetComponentInChildren<Text>().supportRichText = true;
            var extrasList = extras.Split(',');
            if (extrasList.Contains("background")) messageGo.GetComponent<Image>().enabled = true;
        }
    }
}