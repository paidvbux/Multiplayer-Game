using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChatScript : MonoBehaviour
{
    public static ChatScript _singleton;
    public static ChatScript Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(ChatScript)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    [System.Serializable]
    public class Message
    {
        public string user;
        public string text;
        public enum Types { normal, local, wolf };
        public Types messageType;

        public Message (string _user, string _text, Types _type)
        {
            user = _user;
            text = _text;
            messageType = _type;
        }
    }

    [SerializeField] TextMeshProUGUI pregameText;
    [SerializeField] TextMeshProUGUI ingameText;
    [SerializeField] Color localColour;
    [SerializeField] Color wolfColour;
    public List<Message> messages;

    void Awake()
    {
        _singleton = this;    
    }

    public void UpdateMessages() //Run in a function because it does not eat the resources too fast
    {
        Color selectedColour = Color.white; //Create a default colour of the text
        List<string> storedMessages = new List<string>(); //Creates a list of messages
        foreach (Message message in messages)
        {
            switch (message.messageType) //Changes the colour of the message depending on the types
            {
                case Message.Types.normal:
                    break;
                case Message.Types.local:
                    selectedColour = localColour;
                    break;
                case Message.Types.wolf:
                    selectedColour = wolfColour;
                    break;
            }
            string str = "";

            if (message.messageType != Message.Types.normal) //Adds a rich text function which changes part of the text
            {
                str += "<color=#" + ColorUtility.ToHtmlStringRGBA(selectedColour) + ">";
                str += message.user + " > " + message.text;
                str += "</color>";
            }
            else
            {
                str = message.user + " > " + message.text;
            }
            storedMessages.Add(str); //Add the message to the list of messages
        }
        storedMessages.Reverse(); //Reverses the list to make the newer messsages at the bottom
        string text = "";
        foreach (string message in storedMessages) //Arranges the list of messages
        {
            text += message + "\n";
        }

        //Sets the chat text to the text arranged
        pregameText.text = text;
        ingameText.text = text; 
    }
}
