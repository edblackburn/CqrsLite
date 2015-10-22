﻿namespace CqrsLite.Framework.Messages
{
	public interface IHandler<in T> where T: IMessage
    {
        void Handle(T message);
    }
}