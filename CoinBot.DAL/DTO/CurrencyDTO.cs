using CoinBot.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinBot.DAL.DTO
{
    [Serializable]
    public class CurrencyDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public double Price { get; set; }
        public double Multiplier { get; set; }

        public CurrencyDTO()
        {
            Multiplier = 1.0;
        }

        public static CurrencyDTO Convert(Currency cur)
        {
            if (cur == null)
                return null;
            else
                return new CurrencyDTO()
                {
                    Id = cur.Id,
                    Name = cur.Name,
                    Symbol = cur.Symbol,
                    Price = cur.PriceUsd
                };
        }

        public override string ToString()
        {
            return $"{Multiplier} of {Symbol} costs  {Price*Multiplier}$";
        }

        public override bool Equals(object obj)
        {
            var item = obj as CurrencyDTO;

            if (item == null)
            {
                return false;
            }

            return (Id == item.Id && Name == item.Name && Symbol == item.Symbol && Price == item.Price && Multiplier == item.Multiplier);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
