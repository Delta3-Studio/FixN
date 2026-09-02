namespace FixN;
// ReSharper disable InconsistentNaming

public readonly partial struct Fix
{
    public static class Easing
    {
        public static Fix Apply(EasingFn fn, Fix k, Fix value) => Calc(fn, k) * value;

        public static Fix Calc(EasingFn fn, Fix k) => fn switch
        {
            EasingFn.Linear => Linear(k),
            EasingFn.Backward => Backward(k),
            EasingFn.QuadraticIn => Quadratic.In(k),
            EasingFn.QuadraticOut => Quadratic.Out(k),
            EasingFn.QuadraticInOut => Quadratic.InOut(k),
            EasingFn.CubicIn => Cubic.In(k),
            EasingFn.CubicOut => Cubic.Out(k),
            EasingFn.CubicInOut => Cubic.InOut(k),
            EasingFn.QuarticIn => Quartic.In(k),
            EasingFn.QuarticOut => Quartic.Out(k),
            EasingFn.QuarticInOut => Quartic.InOut(k),
            EasingFn.QuinticIn => Quintic.In(k),
            EasingFn.QuinticOut => Quintic.Out(k),
            EasingFn.QuinticInOut => Quintic.InOut(k),
            EasingFn.SinusoidalIn => Sinusoidal.In(k),
            EasingFn.SinusoidalOut => Sinusoidal.Out(k),
            EasingFn.SinusoidalInOut => Sinusoidal.InOut(k),
            EasingFn.ExponentialIn => Exponential.In(k),
            EasingFn.ExponentialOut => Exponential.Out(k),
            EasingFn.ExponentialInOut => Exponential.InOut(k),
            EasingFn.CircularIn => Circular.In(k),
            EasingFn.CircularOut => Circular.Out(k),
            EasingFn.CircularInOut => Circular.InOut(k),
            EasingFn.ElasticIn => Elastic.In(k),
            EasingFn.ElasticOut => Elastic.Out(k),
            EasingFn.ElasticInOut => Elastic.InOut(k),
            EasingFn.BackIn => Back.In(k),
            EasingFn.BackOut => Back.Out(k),
            EasingFn.BackInOut => Back.InOut(k),
            EasingFn.BounceIn => Bounce.In(k),
            EasingFn.BounceOut => Bounce.Out(k),
            EasingFn.BounceInOut => Bounce.InOut(k),
            EasingFn.Constant => Constant(k),
            _ => k,
        };

#pragma warning disable S1121

        public static Fix Linear(Fix k) => k;

        public static Fix Backward(Fix k) => One - k;

        public static Fix Constant(Fix _) => One;

        public static class Quadratic
        {
            public static Fix In(Fix k) => k * k;

            public static Fix Out(Fix k) => k * (Two - k);

            public static Fix InOut(Fix k) =>
                (k *= Two) < One
                    ? Half * k * k
                    : -Half * (((k -= One) * (k - Two)) - One);
        }

        public static class Cubic
        {
            public static Fix In(Fix k)
            {
                var k2 = k * k;
                return k2 * k;
            }

            public static Fix Out(Fix k)
            {
                var k2 = (k -= One) * k;
                return One + (k2 * k);
            }

            public static Fix InOut(Fix k)
            {
                if ((k *= Two) < One)
                {
                    var k2 = k * k;
                    return Half * k2 * k;
                }

                var k2o = (k -= Two) * k;
                return Half * ((k2o * k) + Two);
            }
        }

        public static class Quartic
        {
            public static Fix In(Fix k)
            {
                var k2 = k * k;
                return k2 * k2;
            }

            public static Fix Out(Fix k)
            {
                var k2 = (k -= One) * k;
                return One - (k2 * k2);
            }

            public static Fix InOut(Fix k)
            {
                if ((k *= Two) < One)
                {
                    var k2 = k * k;
                    return Half * k2 * k2;
                }

                var k2o = (k -= Two) * k;
                return -Half * ((k2o * k2o) - Two);
            }
        }

        public static class Quintic
        {
            public static Fix In(Fix k)
            {
                var k2 = k * k;
                return k2 * k2 * k;
            }

            public static Fix Out(Fix k)
            {
                var k2 = (k -= One) * k;
                return One + (k2 * k2 * k);
            }

            public static Fix InOut(Fix k)
            {
                if ((k *= Two) < One)
                {
                    var k2 = k * k;
                    return Half * k2 * k2 * k;
                }

                var k2o = (k -= Two) * k;
                return Half * ((k2o * k2o * k) + Two);
            }
        }

        public static class Sinusoidal
        {
            public static Fix In(Fix k) => One - Cos(k * PiOverTwo);

            public static Fix Out(Fix k) => Sin(k * PiOverTwo);

            public static Fix InOut(Fix k) => Half * (One - Cos(Pi * k));
        }

        public static class Exponential
        {
            public static Fix In(Fix k) =>
                k == Zero
                    ? Zero
                    : Exp2(Ten * (k - One));

            public static Fix Out(Fix k) =>
                k == One
                    ? One
                    : One - Exp2(-Ten * k);

            public static Fix InOut(Fix k)
            {
                if (k == Zero || k == One)
                    return k;

                if ((k *= Two) < One)
                    return Half * Exp2(Ten * (k - One));

                return Half * (-Exp2(-Ten * (k - One)) + Two);
            }
        }

        public static class Circular
        {
            public static Fix In(Fix k) => One - Sqrt(One - (k * k));

            public static Fix Out(Fix k) => Sqrt(One - ((k -= One) * k));

            public static Fix InOut(Fix k) =>
                (k *= Two) < One
                    ? -Half * (Sqrt(One - (k * k)) - One)
                    : Half * (Sqrt(One - ((k -= Two) * k)) + One);
        }

        public static class Elastic
        {
            static readonly Fix _elasticPhaseOffset = Raw(6554); // 0.1
            static readonly Fix _elasticPeriod = Raw(26214); // 0.4
            static readonly Fix _elasticAngularFrequency = Two * Pi / _elasticPeriod;

            public static Fix In(Fix k) =>
                k == Zero || k == One
                    ? k
                    : -Exp2(Ten * (k -= One)) *
                      Sin((k - _elasticPhaseOffset) * _elasticAngularFrequency);

            public static Fix Out(Fix k) =>
                k == Zero || k == One
                    ? k
                    : (Exp2(-Ten * k) *
                       Sin((k - _elasticPhaseOffset) * _elasticAngularFrequency)) + One;

            public static Fix InOut(Fix k) =>
                (k *= Two) < One
                    ? -Half * Exp2(Ten * (k -= One)) *
                      Sin((k - _elasticPhaseOffset) * _elasticAngularFrequency)
                    : (Exp2(-Ten * (k -= One)) *
                       Sin((k - _elasticPhaseOffset) * _elasticAngularFrequency) * Half) + One;
        }

        public static class Back
        {
            static readonly Fix _overshoot = Raw(111514); // 1.70158
            static readonly Fix _overshootDouble = Raw(170060); // 2.5949095

            public static Fix In(Fix k)
            {
                var k2 = k * k;
                return k2 * (((_overshoot + One) * k) - _overshoot);
            }

            public static Fix Out(Fix k)
            {
                var k2 = (k -= One) * k;
                return (k2 * (((_overshoot + One) * k) + _overshoot)) + One;
            }

            public static Fix InOut(Fix k) =>
                (k *= Two) < One
                    ? Half * (k * k * (((_overshootDouble + One) * k) - _overshootDouble))
                    : Half * (((k -= Two) * k * (((_overshootDouble + One) * k) + _overshootDouble)) + Two);
        }

        public static class Bounce
        {
            static readonly Fix _bounceScale = Raw(495616); // 7.5625
            static readonly Fix _bounceOffset1 = Raw(49152); // 0.75
            static readonly Fix _bounceOffset2 = Raw(61440); // 0.9375
            static readonly Fix _bounceOffset3 = Raw(64512); // 0.984375
            static readonly Fix _bouncePhase1End = Raw(23831); // 1 / 2.75
            static readonly Fix _bouncePhase2End = Raw(47663); // 2 / 2.75
            static readonly Fix _bouncePhase2Offset = Raw(35747); // 1.50 / 2.75
            static readonly Fix _bouncePhase3Offset = Raw(53622); // 2.25 / 2.75
            static readonly Fix _bouncePhase4Threshold = Raw(59578); // 2.50 / 2.75
            static readonly Fix _bouncePhase4Offset = Raw(62555); // 2.625 / 2.75

            public static Fix In(Fix k) => One - Out(One - k);

            public static Fix Out(Fix k)
            {
                if (k < _bouncePhase1End)
                    return _bounceScale * k * k;

                if (k < _bouncePhase2End)
                    return (_bounceScale * (k -= _bouncePhase2Offset) * k) + _bounceOffset1;

                if (k < _bouncePhase4Threshold)
                    return (_bounceScale * (k -= _bouncePhase3Offset) * k) + _bounceOffset2;

                return (_bounceScale * (k -= _bouncePhase4Offset) * k) + _bounceOffset3;
            }

            public static Fix InOut(Fix k) =>
                k < Half
                    ? In(k * Two) * Half
                    : (Out((k * Two) - One) * Half) + Half;
        }

#pragma warning restore S1121
    }
}
