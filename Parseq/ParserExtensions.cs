/*
 * Parseq - a monadic parser combinator library for C#
 *
 * Copyright (c) 2012 - 2013 WATANABE TAKAHISA <x.linerlock@gmail.com> All rights reserved.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */
using System;
using System.Linq;
using System.Collections.Generic;

namespace Parseq
{
    public static class ParserExtensions
    {
        public static IReply<TToken, TResult> Run<TToken, TResult>(
            this Parser<TToken, TResult> parser,
            IStream<TToken> stream)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (stream == null)
                throw new ArgumentNullException("stream");

            return parser(stream);
        }

        public static Parser<TToken, T> Where<TToken, T>(
            this Parser<TToken, T> parser,
            Func<T, Boolean> predicate)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            return stream => parser(stream).Where(predicate);
        }

        public static Parser<TToken, U> Select<TToken, T, U>(
            this Parser<TToken, T> parser,
            Func<T, U> selector)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return stream =>
            {
                IReply<TToken, T> reply; T result; ErrorMessage error;
                switch ((reply = parser(stream)).TryGetValue(out result, out error))
                {
                    case ReplyStatus.Success: return Reply.Success<TToken, U>(reply.Stream, selector(result), reply.Messages);
                    case ReplyStatus.Failure: return Reply.Failure<TToken, U>(stream, reply.Messages);
                    default: return Reply.Error<TToken, U>(stream, error, reply.Messages);
                }
            };
        }

        public static Parser<TToken, U> SelectMany<TToken, T, U>(
            this Parser<TToken, T> parser,
            Func<T, Parser<TToken, U>> selector)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return stream =>
            {
                IReply<TToken, T> reply; T result; ErrorMessage error;
                switch ((reply = parser(stream)).TryGetValue(out result, out error))
                {
                    case ReplyStatus.Success: return selector(result).Message(reply.Messages)(reply.Stream);
                    case ReplyStatus.Failure: return Reply.Failure<TToken, U>(stream, reply.Messages);
                    default: return Reply.Error<TToken, U>(stream, error, reply.Messages);
                }
            };
        }

        public static Parser<TToken, V> SelectMany<TToken, T, U, V>(
            this Parser<TToken, T> parser,
            Func<T, Parser<TToken, U>> selector,
            Func<T, U, V> projector)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (selector == null)
                throw new ArgumentNullException("selector");
            if (projector == null)
                throw new ArgumentNullException("projector");

            return parser.SelectMany(x => selector(x).Select(y => projector(x, y)));
        }

        public static Parser<TToken, TResult> Message<TToken, TResult>(
            this Parser<TToken, TResult> parser,
            IEnumerable<ErrorMessage> messages)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (messages == null)
                throw new ArgumentNullException("messages");


            return stream =>
            {
                IReply<TToken, TResult> reply; TResult result; ErrorMessage error;
                switch ((reply = parser(stream)).TryGetValue(out result, out error))
                {
                    case ReplyStatus.Success: return Reply.Success<TToken, TResult>(reply.Stream, result, reply.Messages.Concat(messages));
                    case ReplyStatus.Failure: return Reply.Failure<TToken, TResult>(stream, reply.Messages.Concat(messages));
                    default: return Reply.Error<TToken, TResult>(stream, error, reply.Messages.Concat(messages));
                }
            };
        }

        public static Parser<TToken, TResult> Message<TToken, TResult>(
            this Parser<TToken, TResult> parser,
            String message)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (message == null)
                throw new ArgumentNullException("message");

            return stream => parser.Message(new[]
            {
                new ErrorMessage(ErrorMessageType.Message, message, stream.Position, stream.Position),
            })(stream);
        }
    }
}
