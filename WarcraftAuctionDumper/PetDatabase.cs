using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WowAucDumper
{
    class PetDatabase
    {
        // Use this WeakAura to dump all pets in format "%d\t%d\t%d\n". Then call PetDatabase.Load(file). For details read aura description.
        // !vE1)UTTnq4NLPTa4aK4A7KTbuGnaBhLF05y7fkVGH2slwjkBUktkis1e3Hy0T9gS9I0)ydOadBpdoVr7oszh3ghhV4yzjYpE3XJ39DNO1PbuYekjg)hUvvysfsoL0BqqNZ66tjjkPHiElN2TEdkPqaiBwWotF5ObzF9rucRWmwL3lZiusnicdijHolLnnGFn8WozuY0EjjAUH2TgLKZhbidMMbkXyrO5Pj9vcjCBREbb9oheQmceA5GT97g4FbLevOnQjOuhKfZmW6lk)fSb1eMreDPi2mgnqJcgnAHnr0gwUXzCcPaVJeRg6eiT1crtjdpPkXGcQVkRi7iblvns)CVJgCE)H99diEVCw43ml8NN9SzHF8hCRyNZBhT3E3F(xvymkzdlK2D6r8xdMXmTFSW0sDTfMjVGVEuW2KVfi5oiwNIf3bFzT1allNNWZZ5XNjJ5obEWAGzet4q4HD(1jMRgls5hXzXB04blVN0xhXY4BcxpjzS6klIKcP9GScgNS79HU4do9tjCJD7w5Wg12a2uvelDPdYQMHN8CReoHB6YMWRS7SWQvHJZv814PVk)HHEhShuXLAeTtmq2UNQIXo2ldz5JAS7wT6ZlsnIoqQAf0dUDR5yfeNxzlWEQy04u4RZgtyPA(J7mDr4B0x2YLeSjhKtkpTDkNL3mn1sbSrBUCblp4BuR2wH2k5kEoYfiJDXMhUBLbHa9W9pyDsKlJxtCBziWIq8(5CTMh))pog8C9z5CWc3fonI5v2AlONeXB1OuKUwanJI4zMncPnWaZtxa5oe3GuOjahQESLofEYKlgnINJ8SDTLtkhWERLMFb7QU4v83a7jsrsI4AGTTDtsWqsqZlaO2zOKt5SuKcFbwWdAXs673PZcrnePraPdmsq5iDgpn9S4YIp2vPHQgN2my45KtgsA(JlxNZEkvLecnlxubwuGGfTa7EP9U8eZUG9WG4QRCoS)(z52yOJ8Bn4eBi0AXjskNbCNybIvTmpGbCmxE)JGYKQj6r7HHeaZCU90ae99bJAaq(uOgy04kEZU5ZIlMKLXn6VWB3hsd3r)72eWkeYrZczPGEFTuDfKlJIW7Xt9HQWmxXpVhgBcsDkC5c1Hnv9AWFWAvp8s(e1awtz53W2d7ZnptvKlzPvXKfU5mzIQ1usgpsW1NDufrmS1V5Xfo68(CRO)wWOEmV169EcitenVNxVSeH3qVYro8L7U9sBPJudjrYrvbFgCMwXBN4xy2rdFJFH02Fr4g14EojTfkgcS2q5Gnn5k9jneRvxzLUKqo0VRBVl7olCXZpG98rQW(aCbAoJ)kGJW1K4P(D6F8GoU0074wSCpylEVHh4g8Cfqk0D)61CCuxZJDTd2fkjG99z4y0YpauvqonWv9vuYpbz6IKPush)JdW2ofyBKWSR0ZyjjMuHTfhxKBbuYKO5rkjW8akCcti3E0lPq3o8yFRGBbZrbxkh53CTiFCoqI58tK2x473fenoeqcdoCKqg8L6ikzGgQjSKwaIFGS7Na)AuWzJvS28ECsOsDRPZcVq8AUbdhFYSW5)(8)623D7Vm)9Z)75F42FD(73F(Fm)FV9DZ)ZB)T5)d85duYvQ84lZzq)(xU8o8ThOKJZfVDw43xWIZTb4bW(g2AjIro63Rx59dWTSyLxMivXSVzcWYhzFzfSXNfN)rPmT(EJQXxx5tgebRsv5UQu4)nSxpWE9q8QBTusJQ1pO6HuYBGbpSboo9)(

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public static Dictionary<int, Pet> DB = new Dictionary<int, Pet>();

        public class Pet
        {
            public int SpeciesId = 0;
            public string Name = "";
            public int NpcId = 0;

            public override string ToString()
            {
                return $"[{Name}] {SpeciesId} {NpcId}";
            }
        }

        private PetDatabase()
        {

        }

        public static void Clear()
        {
            DB.Clear();
        }

        public static void Load(string filepath)
        {
            if (DB.Count > 0) return;

            using (StreamReader reader = new StreamReader(filepath, Encoding.UTF8))
                while (reader.Peek() >= 0)
                {
                    string[] data = reader.ReadLine().Split('\t');
                    if (data.Length == 3)
                    {
                        Pet pet = new Pet()
                        {
                            SpeciesId = int.Parse(data[0]),
                            Name = data[1],
                            NpcId = int.Parse(data[2]),
                        };

                        DB.Add(pet.SpeciesId, pet);
                        //logger.Debug("Pet added to database: {0}", pet.ToString());
                    }
                }
            logger.Info("Added {0} pets to database", DB.Count);
        }
    }
}
