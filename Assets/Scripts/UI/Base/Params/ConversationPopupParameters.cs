using System;
using System.Collections.Generic;

public class ConversationPopupParameters : UIBaseParameters
{
    public List<ConversationData.Data> Conversation = new();
    public Action OnEndConversation;
    public Action<int> OnNextConversation;
}