﻿/*
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

namespace Parseq.Combinators
{
    public static class Prims
    {
        public static Parser<TToken, Unit> Eof<TToken>()
        {
            return stream => stream.CanNext()
                ? Reply.Failure<TToken, Unit>(stream)
                : Reply.Success<TToken, Unit>(stream, Unit.Instance);
        }

        public static Parser<TToken, TResult> Return<TToken, TResult>(this TResult value)
        {
            return stream => Reply.Success<TToken, TResult>(stream, value);
        }

        public static Parser<TToken, TResult> Return<TToken, TResult>(this Func<TResult> func)
        {
            if (func == null)
                throw new ArgumentNullException("func");

            return stream => Reply.Success<TToken, TResult>(stream, func());
        }

        public static Parser<TToken, IEnumerable<TResult>> Empty<TToken, TResult>()
        {
            return Prims.Return<TToken, IEnumerable<TResult>>(Enumerable.Empty<TResult>());
        }

        public static Parser<TToken, TResult> Fail<TToken, TResult>()
        {
            return stream => Reply.Failure<TToken, TResult>(stream);
        }

        public static Parser<TToken, TResult> Error<TToken, TResult>(String message)
        {
            return Errors.Error<TToken, TResult>(message);
        }

        public static Parser<TToken, TResult> Error<TToken, TResult>()
        {
            return Prims.Error<TToken, TResult>("Unspecified Error");
        }

        public static Parser<TToken, TToken> Satisfy<TToken>(this Func<TToken, Boolean> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException("selector");

            TToken value;
            return stream => stream.Current.TryGetValue(out value) && predicate(value)
                    ? Reply.Success<TToken, TToken>(stream.Next(), value)
                    : Reply.Failure<TToken, TToken>(stream);
        }

        public static Parser<TToken, TToken> Satisfy<TToken>(this TToken token)
            where TToken : IEquatable<TToken>
        {
            return Prims.Satisfy<TToken>(t => t.Equals(token));
        }

        public static Parser<TToken, TToken> Any<TToken>()
        {
            return Prims.Satisfy<TToken>(_ => true);
        }

        public static Parser<TToken, TToken> OneOf<TToken>(
            Func<TToken, TToken, Boolean> predicate,
            IEnumerable<TToken> candidates)
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");
            if (candidates == null)
                throw new ArgumentNullException("candidates");

            return Combinator.Choice(candidates.Select(x => Prims.Satisfy<TToken>(y => predicate(x, y))));
        }

        public static Parser<TToken, TToken> OneOf<TToken>(
            Func<TToken, TToken, Boolean> predicate,
            params TToken[] candidates)
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");
            if (candidates == null)
                throw new ArgumentNullException("candidates");

            return Prims.OneOf<TToken>(predicate, candidates.AsEnumerable());
        }

        public static Parser<TToken, TToken> OneOf<TToken>(IEnumerable<TToken> candidates)
            where TToken : IEquatable<TToken>
        {
            return Prims.OneOf<TToken>((x, y) => x.Equals(y), candidates);
        }

        public static Parser<TToken, TToken> OneOf<TToken>(params TToken[] candidates)
            where TToken : IEquatable<TToken>
        {
            return Prims.OneOf<TToken>(candidates.AsEnumerable());
        }

        public static Parser<TToken, TToken> NoneOf<TToken>(
            Func<TToken, TToken, Boolean> predicate, IEnumerable<TToken> candidates)
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");
            if (candidates == null)
                throw new ArgumentNullException("candidates");

            return Combinator.Choice(candidates.Select(x => Prims.Satisfy<TToken>(y => predicate(x, y))))
                .Not()
                .Right(Prims.Any<TToken>());
        }

        public static Parser<TToken, TToken> NoneOf<TToken>(
            Func<TToken, TToken, Boolean> predicate, params TToken[] candidates)
        {
            if (candidates == null)
                throw new ArgumentNullException("candidates");

            return Prims.NoneOf<TToken>(predicate, candidates.AsEnumerable());
        }

        public static Parser<TToken, TToken> NoneOf<TToken>(IEnumerable<TToken> candidates)
            where TToken : IEquatable<TToken>
        {
            return Prims.NoneOf<TToken>((x, y) => x.Equals(y), candidates);
        }

        public static Parser<TToken, TToken> NoneOf<TToken>(params TToken[] candidates)
            where TToken : IEquatable<TToken>
        {
            return Prims.NoneOf<TToken>(candidates.AsEnumerable());
        }

        public static Parser<TToken, TResult2> Pipe<TToken, TResult0, TResult1, TResult2>(
            this Parser<TToken, TResult0> parser0,
            Parser<TToken, TResult1> parser1,
            Func<TResult0, TResult1, TResult2> selector)
        {
            if (parser0 == null)
                throw new ArgumentNullException("parser0");
            if (parser1 == null)
                throw new ArgumentNullException("parser1");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return from x in parser0
                   from y in parser1
                   select selector(x, y);
        }

        public static Parser<TToken, TResult3> Pipe<TToken, TResult0, TResult1, TResult2, TResult3>(
            this Parser<TToken, TResult0> parser0,
            Parser<TToken, TResult1> parser1,
            Parser<TToken, TResult2> parser2,
            Func<TResult0, TResult1, TResult2, TResult3> selector)
        {
            if (parser0 == null)
                throw new ArgumentNullException("parser0");
            if (parser1 == null)
                throw new ArgumentNullException("parser1");
            if (parser2 == null)
                throw new ArgumentNullException("parser2");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return from x in parser0
                   from y in parser1
                   from z in parser2
                   select selector(x, y, z);
        }

        public static Parser<TToken, TResult4> Pipe<TToken, TResult0, TResult1, TResult2, TResult3, TResult4>(
            this Parser<TToken, TResult0> parser0,
            Parser<TToken, TResult1> parser1,
            Parser<TToken, TResult2> parser2,
            Parser<TToken, TResult3> parser3,
            Func<TResult0, TResult1, TResult2, TResult3, TResult4> selector)
        {
            if (parser0 == null)
                throw new ArgumentNullException("parser0");
            if (parser1 == null)
                throw new ArgumentNullException("parser1");
            if (parser2 == null)
                throw new ArgumentNullException("parser2");
            if (parser3 == null)
                throw new ArgumentNullException("parser3");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return from x in parser0
                   from y in parser1
                   from z in parser2
                   from a in parser3
                   select selector(x, y, z, a);
        }

        public static Parser<TToken, TResult5> Pipe<TToken, TResult0, TResult1, TResult2, TResult3, TResult4, TResult5>(
            this Parser<TToken, TResult0> parser0,
            Parser<TToken, TResult1> parser1,
            Parser<TToken, TResult2> parser2,
            Parser<TToken, TResult3> parser3,
            Parser<TToken, TResult4> parser4,
            Func<TResult0, TResult1, TResult2, TResult3, TResult4, TResult5> selector)
        {
            if (parser0 == null)
                throw new ArgumentNullException("parser0");
            if (parser1 == null)
                throw new ArgumentNullException("parser1");
            if (parser2 == null)
                throw new ArgumentNullException("parser2");
            if (parser3 == null)
                throw new ArgumentNullException("parser3");
            if (parser4 == null)
                throw new ArgumentNullException("parser4");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return from x in parser0
                   from y in parser1
                   from z in parser2
                   from a in parser3
                   from b in parser4
                   select selector(x, y, z, a, b);
        }

        public static Parser<TToken, TResult6> Pipe<TToken, TResult0, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(
            this Parser<TToken, TResult0> parser0,
            Parser<TToken, TResult1> parser1,
            Parser<TToken, TResult2> parser2,
            Parser<TToken, TResult3> parser3,
            Parser<TToken, TResult4> parser4,
            Parser<TToken, TResult5> parser5,
            Func<TResult0, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6> selector)
        {
            if (parser0 == null)
                throw new ArgumentNullException("parser0");
            if (parser1 == null)
                throw new ArgumentNullException("parser1");
            if (parser2 == null)
                throw new ArgumentNullException("parser2");
            if (parser3 == null)
                throw new ArgumentNullException("parser3");
            if (parser4 == null)
                throw new ArgumentNullException("parser4");
            if (parser5 == null)
                throw new ArgumentNullException("parser5");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return from x in parser0
                   from y in parser1
                   from z in parser2
                   from a in parser3
                   from b in parser4
                   from c in parser5
                   select selector(x, y, z, a, b, c);
        }

        public static Parser<TToken, TResult7> Pipe<TToken, TResult0, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(
            this Parser<TToken, TResult0> parser0,
            Parser<TToken, TResult1> parser1,
            Parser<TToken, TResult2> parser2,
            Parser<TToken, TResult3> parser3,
            Parser<TToken, TResult4> parser4,
            Parser<TToken, TResult5> parser5,
            Parser<TToken, TResult6> parser6,
            Func<TResult0, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7> selector)
        {
            if (parser0 == null)
                throw new ArgumentNullException("parser0");
            if (parser1 == null)
                throw new ArgumentNullException("parser1");
            if (parser2 == null)
                throw new ArgumentNullException("parser2");
            if (parser3 == null)
                throw new ArgumentNullException("parser3");
            if (parser4 == null)
                throw new ArgumentNullException("parser4");
            if (parser5 == null)
                throw new ArgumentNullException("parser5");
            if (parser6 == null)
                throw new ArgumentNullException("parser6");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return from x in parser0
                   from y in parser1
                   from z in parser2
                   from a in parser3
                   from b in parser4
                   from c in parser5
                   from d in parser6
                   select selector(x, y, z, a, b, c, d);
        }

        public static Parser<TToken, TResult8> Pipe<TToken, TResult0, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(
            this Parser<TToken, TResult0> parser0,
            Parser<TToken, TResult1> parser1,
            Parser<TToken, TResult2> parser2,
            Parser<TToken, TResult3> parser3,
            Parser<TToken, TResult4> parser4,
            Parser<TToken, TResult5> parser5,
            Parser<TToken, TResult6> parser6,
            Parser<TToken, TResult7> parser7,
            Func<TResult0, TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8> selector)
        {
            if (parser0 == null)
                throw new ArgumentNullException("parser0");
            if (parser1 == null)
                throw new ArgumentNullException("parser1");
            if (parser2 == null)
                throw new ArgumentNullException("parser2");
            if (parser3 == null)
                throw new ArgumentNullException("parser3");
            if (parser4 == null)
                throw new ArgumentNullException("parser4");
            if (parser5 == null)
                throw new ArgumentNullException("parser5");
            if (parser6 == null)
                throw new ArgumentNullException("parser6");
            if (parser7 == null)
                throw new ArgumentNullException("parser7");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return from x in parser0
                   from y in parser1
                   from z in parser2
                   from a in parser3
                   from b in parser4
                   from c in parser5
                   from d in parser6
                   from e in parser7
                   select selector(x, y, z, a, b, c, d, e);
        }

        public static Parser<TToken, TResult0> Left<TToken, TResult0, TResult1>(
            this Parser<TToken, TResult0> parser0,
            Parser<TToken, TResult1> parser1)
        {
            if (parser0 == null)
                throw new ArgumentNullException("parser0");
            if (parser1 == null)
                throw new ArgumentNullException("parser1");

            return from x in parser0
                   from _ in parser1
                   select x;
        }

        public static Parser<TToken, TResult1> Right<TToken, TResult0, TResult1>(
            this Parser<TToken, TResult0> parser0,
            Parser<TToken, TResult1> parser1)
        {
            if (parser0 == null)
                throw new ArgumentNullException("parser0");
            if (parser1 == null)
                throw new ArgumentNullException("parser1");

            return from _ in parser0
                   from y in parser1
                   select y;
        }

        public static Parser<TToken, Tuple<TResult0, TResult1>> Both<TToken, TResult0, TResult1>(
            this Parser<TToken, TResult0> parser0,
            Parser<TToken, TResult1> parser1)
        {
            if (parser0 == null)
                throw new ArgumentNullException("parser0");
            if (parser1 == null)
                throw new ArgumentNullException("parser1");

            return from x in parser0
                   from y in parser1
                   select Tuple.Create(x, y);
        }

        public static Parser<TToken, TResult0> Between<TToken, TResult0, TResult1, TResult2>(
            this Parser<TToken, TResult0> parser0,
            Parser<TToken, TResult1> parser1,
            Parser<TToken, TResult2> parser2)
        {
            if (parser0 == null)
                throw new ArgumentNullException("parser0");
            if (parser1 == null)
                throw new ArgumentNullException("parser1");
            if (parser2 == null)
                throw new ArgumentNullException("parser2");

            return from x in parser1
                   from y in parser0
                   from z in parser2
                   select y;
        }

        public static Parser<TToken, IEnumerable<TResult>> SepBy<TToken, TResult, TSeparator>(
            this Parser<TToken, TResult> parser,
            Int32 count,
            Parser<TToken, TSeparator> separator)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if (separator == null)
                throw new ArgumentNullException("separator");


            return parser.Replicate().Select((p, i) => (i == 0) ? p : separator.Right(p))
                .Partition(count)
                .Case((par0, par1) => Combinator.Sequence(par0).SelectMany(x => Combinator.Greed(par1).Select(y => x.Concat(y))));
        }

        public static Parser<TToken, IEnumerable<TResult>> SepBy<TToken, TResult, TSeparator>(
            this Parser<TToken, TResult> parser,
            Parser<TToken, TSeparator> separator)
        {
            return parser.SepBy(0, separator);
        }

        public static Parser<TToken, IEnumerable<TResult>> EndBy<TToken, TResult, TSeparator>(
            this Parser<TToken, TResult> parser,
            Int32 count,
            Parser<TToken, TSeparator> separator)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if (separator == null)
                throw new ArgumentNullException("separator");

            return parser.Left(separator).Many(count);
        }

        public static Parser<TToken, IEnumerable<TResult>> EndBy<TToken, TResult, TSeparator>(
            this Parser<TToken, TResult> parser,
            Parser<TToken, TSeparator> separator)
        {
            return Prims.EndBy(parser, 0, separator);
        }

        public static Parser<TToken, IEnumerable<TResult>> SepEndBy<TToken, TResult, TSeparator>(
            this Parser<TToken, TResult> parser,
            Int32 count,
            Parser<TToken, TSeparator> separator)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if (separator == null)
                throw new ArgumentNullException("separator");

            return Prims.SepBy(parser, count, separator).Left(separator.Maybe());
        }

        public static Parser<TToken, IEnumerable<TResult>> SepEndBy<TToken, TResult, TSeparator>(
            this Parser<TToken, TResult> parser,
            Parser<TToken, TSeparator> separator)
        {
            return parser.SepEndBy(0, separator);
        }

        public static Parser<TToken, TResult1> Chainl<TToken, TResult0, TResult1>(
            this Parser<TToken, TResult0> parser,
            Parser<TToken, Unit> separator,
            TResult1 seed,
            Func<TResult1, TResult0, TResult1> selector)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (separator == null)
                throw new ArgumentNullException("separator");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return parser.Pipe(separator.Right(parser).Many(1),
                (head, tail) => tail.Foldl(selector(seed, head), selector));
        }

        public static Parser<TToken, TResult1> Chainl<TToken, TResult0, TResult1>(
            this Parser<TToken, TResult0> parser,
            Parser<TToken, Unit> separator,
            Func<TResult0, TResult1> seedSelector,
            Func<TResult1, TResult0, TResult1> restSelector)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (separator == null)
                throw new ArgumentNullException("separator");
            if (seedSelector == null)
                throw new ArgumentNullException("seed");
            if (restSelector == null)
                throw new ArgumentNullException("selector");

            return parser.Pipe(separator.Right(parser).Many(1),
                (head, tail) => tail.Foldl(seedSelector(head), restSelector));
        }

        public static Parser<TToken, TResult> Chainl<TToken, TResult>(
            this Parser<TToken, TResult> parser,
            Parser<TToken, Unit> separator,
            Func<TResult, TResult, TResult> selector)
        {
            return Prims.Chainl<TToken, TResult, TResult>(parser, separator, _ => _, selector);
        }

        public static Parser<TToken, TResult1> Chainr<TToken, TResult0, TResult1>(
            this Parser<TToken, TResult0> parser,
            Parser<TToken, Unit> separator,
            TResult1 seed,
            Func<TResult0, TResult1, TResult1> selector)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (separator == null)
                throw new ArgumentNullException("separator");
            if (seed == null)
                throw new ArgumentNullException("seed");
            if (selector == null)
                throw new ArgumentNullException("selector");

            return parser.Pipe(separator.Right(parser).Many(1),
                (head, tail) => head.Concat(tail)
                    .LastAndInit()
                    .Case((last, init) => init.Foldr(selector(last, seed), (x, y) => selector(x, y))));
        }

        public static Parser<TToken, TResult1> Chainr<TToken, TResult0, TResult1>(
            this Parser<TToken, TResult0> parser,
            Parser<TToken, Unit> separator,
            Func<TResult0, TResult1> seedSelector,
            Func<TResult0, TResult1, TResult1> restSelector)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (separator == null)
                throw new ArgumentNullException("separator");
            if (seedSelector == null)
                throw new ArgumentNullException("seed");
            if (restSelector == null)
                throw new ArgumentNullException("selector");

            return parser.Pipe(separator.Right(parser).Many(1),
                (head, tail) => head.Concat(tail)
                    .LastAndInit()
                    .Case((last, init) => init.Foldr(seedSelector(last), (x, y) => restSelector(x, y))));
        }

        public static Parser<TToken, TResult> Chainr<TToken, TResult>(
            this Parser<TToken, TResult> parser,
            Parser<TToken, Unit> separator,
            Func<TResult, TResult, TResult> selector)
        {
            return Prims.Chainr<TToken, TResult, TResult>(parser, separator, _ => _, selector);
        }

        public static Parser<TToken, TResult> WhenSuccess<TToken, TResult>(
            this Parser<TToken, TResult> parser0, Parser<TToken, TResult> parser1)
        {
            if (parser0 == null)
                throw new ArgumentNullException("parser0");
            if (parser1 == null)
                throw new ArgumentNullException("parser1");

            return stream =>
            {
                IReply<TToken, TResult> reply;
                TResult result; ErrorMessage error;
                switch ((reply = parser0(stream)).TryGetValue(out result, out error))
                {
                    case ReplyStatus.Success:
                        return parser1(reply.Stream);
                    case ReplyStatus.Failure:
                        return Reply.Failure<TToken, TResult>(stream);
                    default:
                        return Reply.Error<TToken, TResult>(stream, error);
                }
            };
        }

        public static Parser<TToken, TResult> WhenFailure<TToken, TResult>(
            this Parser<TToken, TResult> parser0, Parser<TToken, TResult> parser1)
        {
            if (parser0 == null)
                throw new ArgumentNullException("parser0");
            if (parser1 == null)
                throw new ArgumentNullException("parser1");

            return stream =>
            {
                IReply<TToken, TResult> reply;
                TResult result; ErrorMessage error;
                switch ((reply = parser0(stream)).TryGetValue(out result, out error))
                {
                    case ReplyStatus.Success:
                        return Reply.Success<TToken, TResult>(reply.Stream, result);
                    case ReplyStatus.Failure:
                        return parser1(stream);
                    default:
                        return Reply.Error<TToken, TResult>(stream, error);
                }
            };
        }

        public static Parser<TToken, TResult> WhenError<TToken, TResult>(
            this Parser<TToken, TResult> parser0, Parser<TToken, TResult> parser1)
        {
            if (parser0 == null)
                throw new ArgumentNullException("parser0");
            if (parser1 == null)
                throw new ArgumentNullException("parser1");

            return stream =>
            {
                IReply<TToken, TResult> reply;
                TResult result; ErrorMessage error;
                switch ((reply = parser0(stream)).TryGetValue(out result, out error))
                {
                    case ReplyStatus.Success:
                        return Reply.Success<TToken, TResult>(reply.Stream, result);
                    case ReplyStatus.Failure:
                        return Reply.Failure<TToken, TResult>(stream);
                    default:
                        return parser1(stream);
                }
            };
        }
    }
}
