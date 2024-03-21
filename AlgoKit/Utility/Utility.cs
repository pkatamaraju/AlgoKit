using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlgoKit.Data.Core.Models;
using KiteConnect;

namespace AlgoKit.Data.Core
{
    public class Utility
    {

        public List<InstrumentPosition> getInstrumentPosition(List<Position> positions)
        {

            List<InstrumentPosition> instrumentPositions = new List<InstrumentPosition>();
            InstrumentPosition instrumentPosition = null; ;

            foreach(Position position in positions)
            {
                instrumentPosition = new InstrumentPosition();

                instrumentPosition.InstrumentToken = position.InstrumentToken;
                instrumentPosition.Status = "OPEN";
                instrumentPosition.Time = DateTime.Now;
                instrumentPosition.Quantity= position.Quantity;
                instrumentPosition.TriggerPrice = 0;

                instrumentPositions.Add(instrumentPosition);
            }
            return instrumentPositions;
        }


        static DateTime FindNextWednesday(DateTime date)
        {
            // Find the day of the week of the given date
            DayOfWeek dayOfWeek = date.DayOfWeek;

            // Calculate the number of days to add to reach the next Wednesday
            int daysToAdd = ((int)DayOfWeek.Wednesday - (int)dayOfWeek + 7) % 7;

            // Add the calculated number of days to the given date to get the next Wednesday
            DateTime nextWednesday = date.AddDays(daysToAdd);

            return nextWednesday;
        }

        static DateTime FindNextThursday(DateTime date)
        {
            // Find the day of the week of the given date
            DayOfWeek dayOfWeek = date.DayOfWeek;

            // Calculate the number of days to add to reach the next Wednesday
            int daysToAdd = ((int)DayOfWeek.Thursday - (int)dayOfWeek + 7) % 7;

            // Add the calculated number of days to the given date to get the next Wednesday
            DateTime nextWednesday = date.AddDays(daysToAdd);

            return nextWednesday;
        }

    }
}
