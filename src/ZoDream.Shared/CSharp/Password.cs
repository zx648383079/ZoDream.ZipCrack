using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.CSharp
{
    public class PasswordRecovery
    {
        uint[] x = new uint[7];
        uint[] y = new uint[7];
        uint[] z = new uint[7];

        byte[] p = new byte[6];

        bool[] zm1_24_32 = new bool[1 << 8];
        bool[] z0_16_32 = new bool[1 << 16];
        uint x0;

        readonly IList<byte> Charset;
        public bool ShouldStop { get; set; } = false;
        public string Password { get; set; }

        public PasswordRecovery(Keys keys, IList<byte> charset)
        {
            Charset = charset;
            // initialize target X, Y and Z values
            x[6] = keys.X;
            y[6] = keys.Y;
            z[6] = keys.Z;

            // derive Y5
            y[5] = (y[6] - 1) * MultTab.MULTINV - Util.Lsb(x[6]);

            // derive more Z bytes
            for (int i = 6; 1 < i; i--)
            {
                z[i - 1] = Crc32Tab.Crc32Inv(z[i], Util.Msb(y[i]));
            }

            // precompute possible Z0[16,32) and Z{-1}[24,32)
            foreach (var p5 in charset)
            {
                x[5] = Crc32Tab.Crc32Inv(x[6], p5);
                y[4] = (y[5] - 1) * MultTab.MULTINV - Util.Lsb(x[5]);
                z[3] = Crc32Tab.Crc32Inv(z[4], Util.Msb(y[4]));

                foreach (var p4 in charset)
                {
                    x[4] = Crc32Tab.Crc32Inv(x[5], p4);
                    y[3] = (y[4] - 1) * MultTab.MULTINV - Util.Lsb(x[4]);
                    z[2] = Crc32Tab.Crc32Inv(z[3], Util.Msb(y[3]));
                    z[1] = Crc32Tab.Crc32Inv(z[2], 0);
                    z[0] = Crc32Tab.Crc32Inv(z[1], 0);

                    z0_16_32[z[0] >> 16] = true;
                    zm1_24_32[Crc32Tab.Crc32Inv(z[0], 0) >> 24] = true;
                }
            }
        }

        public bool RecoverShortPassword()
        {
            var initial = new Keys();

            for (int length = 6; length >= 0; length--)
            {
                if (Recover(initial))
                {
                    Password = Password.Remove(0, 6 - length);
                    return true;
                }

                initial.UpdateBackwardPlaintext(Charset[0]);
            }

            return false;
        }

        public bool RecoverLongPassword(Keys initial, int length)
        {
            if (length == 7)
            {
                if (!zm1_24_32[initial.Z >> 24])
                {
                    return false;
                }

                foreach (var pi in Charset)
                {
                    Keys init = initial;
                    init.Update(pi);

                    if (Recover(init))
                    {
                        Password = Password.Insert(0, Encoding.UTF8.GetString(new byte[] { pi }));
                        return true;
                    }
                }
            }
            else
            {
                if (ShouldStop)
                {
                    return false;
                }

                foreach (var pi in Charset)
                {
                    var init = initial;
                    init.Update(pi);

                    if (RecoverLongPassword(init, length - 1))
                    {
                        Password = Password.Insert(0, Encoding.UTF8.GetString(new byte[] { pi }));
                        return true;
                    }
                }
            }

            return false;
        }


        public bool Recover(Keys initial)
        {
            // check compatible Z0[16,32)
            if (!z0_16_32[initial.Z >> 16])
            {
                return false;
            }

            // initialize starting X, Y and Z values
            x[0] = x0 = initial.X;
            y[0] = initial.Y;
            z[0] = initial.Z;

            // complete Z values and derive Y[24,32) values
            for (int i = 1; i <= 4; i++)
            {
                y[i] = Crc32Tab.GetYi_24_32(z[i], z[i - 1]);
                z[i] = Crc32Tab.Crc32(z[i - 1], Util.Msb(y[i]));
            }

            // recursively complete Y values and derive password
            return Recursion(5);
        }


        public bool Recursion(int i)
        {
            if (i != 1) // the Y-list is not complete so generate Y{i-1} values
            {
                var fy = (y[i] - 1) * MultTab.MULTINV;
                var ffy = (fy - 1) * MultTab.MULTINV;

                // get possible LSB(Xi)
                foreach (var xi_0_8 in MultTab.GetMsbProdFiber2(Util.Msb(ffy - (y[i - 2] & Util.MASK_24_32))))
                {
                    // compute corresponding Y{i-1}
                    var yim1 = fy - xi_0_8;

                    // filter values with Y{i-2}[24,32)
                    if (ffy - MultTab.GetMultInv(xi_0_8) - (y[i - 2] & Util.MASK_24_32) <= Util.MAXDIFF_0_24
                        && Util.Msb(yim1) == Util.Msb(y[i - 1]))
                    {
                        // add Y{i-1} to the Y-list
                        y[i - 1] = yim1;

                        // set Xi value
                        x[i] = xi_0_8;

                        if (Recursion(i - 1))
                        {
                            return true;
                        }
                    }
                }
            }
            else // the Y-list is complete
            {
                // only the X1 LSB was not set yet, so do it here
                x[1] = (y[1] - 1) * MultTab.MULTINV - y[0];
                if (x[1] > 0xff)
                {
                    return false;
                }

                // complete X values and derive password
                for (int j = 5; 0 <= j; j--)
                {
                    var xi_xor_pi = Crc32Tab.Crc32Inv(x[j + 1], 0);
                    p[j] = Util.Lsb(xi_xor_pi ^ x[j]);
                    x[j] = xi_xor_pi ^ p[j];
                }

                if (x[0] == x0) // the password is successfully recovered
                {
                    Password = Encoding.UTF8.GetString(p);
                    ShouldStop = true;
                    return true;
                }
            }

            return false;
        }

        public static string Recover(Keys keys, int maxLength, IList<byte> charset, 
            ILogger? logger, CancellationToken cancelToken = default)
        {
            var worker = new PasswordRecovery(keys, charset);
            string password = string.Empty;

            // look for a password of length between 0 and 6
            logger?.Info("length 0-6...");
            if (worker.RecoverShortPassword())
            {
                return worker.Password;
            }

            // look for a password of length between 7 and 9
            for (var length = 7; length < 10 && length <= maxLength; length++)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return string.Empty;
                }
                // std::cout << "length " << length << "..." << std::endl;
                logger?.Info($"length {length}...");
                if (worker.RecoverLongPassword(new Keys(), length))
                {
                    return worker.Password;
                }
            }

            // look for a password of length between 10 and max_length
            // same as above, but in a parallel loop
            for (var length = 10; length <= maxLength; length++)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return string.Empty;
                }
                // std::cout << "length " << length << "..." << std::endl;
                logger?.Info($"length {length}...");
                var charsetSize = charset.Count;
                var progress = 0;
                var total = charsetSize * charsetSize;
                logger?.Progress(progress, total);
                // bruteforce two characters to have many tasks for each CPU thread and share work evenly
#pragma omp parallel for firstprivate(worker) schedule(dynamic)
                for (var i = 0; i < total; i++)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        return string.Empty;
                    }
                    if (worker.ShouldStop)
                    {
                        continue; // cannot break out of an OpenMP for loop
                    }

                    var init = new Keys();
                    init.Update(charset[i / charsetSize]);
                    init.Update(charset[i % charsetSize]);
                    if (cancelToken.IsCancellationRequested)
                    {
                        return string.Empty;
                    }
                    if (worker.RecoverLongPassword(init, length - 2))
                    {
                        password = worker.Password;
                        password = password.Insert(0, Encoding.UTF8.GetString(new byte[] { charset[i % charsetSize] }));
                        password = password.Insert(0, Encoding.UTF8.GetString(new byte[] { charset[i / charsetSize] }));
                    }

#pragma omp critical
                    logger?.Progress(++progress, total);
                    // std::cout << progress(++done, charsetSize* charsetSize) << std::flush << "\r";
                }

                /// std::cout << std::endl;

                if (worker.ShouldStop)
                {
                    return password;
                }
            }
            return string.Empty;
        }
    }
}
