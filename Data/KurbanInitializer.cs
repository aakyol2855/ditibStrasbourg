using DitibStasbourg.Data;
using DitibStasbourg.Models;
using Microsoft.EntityFrameworkCore;

namespace DitibStasbourg.Data
{
    public static class KurbanInitializer
    {
        public static async Task SeedKurbanLookupsAsync(ApplicationDbContext context)
        {
            // 1. Kurban Status
            var statusType = await GetOrCreateTypeAsync(context, "KURBAN_STATUS", "Kurban Durumları");
            await GetOrCreateValueAsync(context, statusType.Id, "Satışta", "Available", 1);
            await GetOrCreateValueAsync(context, statusType.Id, "Dolu", "Full", 2);
            await GetOrCreateValueAsync(context, statusType.Id, "Kesildi", "Slaughtered", 3);

            // 2. Species
            var speciesType = await GetOrCreateTypeAsync(context, "KURBAN_SPECIES", "Kurban Türleri");
            await GetOrCreateValueAsync(context, speciesType.Id, "Büyükbaş", "Cattle", 1);
            await GetOrCreateValueAsync(context, speciesType.Id, "Küçükbaş", "Sheep", 2);

            // 3. Payment Status
            var paymentType = await GetOrCreateTypeAsync(context, "PAYMENT_STATUS", "Ödeme Durumları");
            await GetOrCreateValueAsync(context, paymentType.Id, "Bekliyor", "Pending", 1);
            await GetOrCreateValueAsync(context, paymentType.Id, "Ödendi", "Paid", 2);

            // 4. System Settings (New Rule: Auto-integration)
            var settingsType = await GetOrCreateTypeAsync(context, "SYSTEM_SETTINGS", "Sistem Ayarları");
            await GetOrCreateValueAsync(context, settingsType.Id, "Kurban Yılı", "KURBAN_YILI", 1, "2026");
            await GetOrCreateValueAsync(context, settingsType.Id, "Büyükbaş Hisse Fiyatı", "HISSE_FIYATI_BUYUKBAS", 2, "450");
            await GetOrCreateValueAsync(context, settingsType.Id, "Kesim Sırası Aktif Mi?", "KESIM_SIRASI_AKTIF", 3, "True");
            
            await context.SaveChangesAsync();
        }

        private static async Task<LookupType> GetOrCreateTypeAsync(ApplicationDbContext context, string code, string name)
        {
            var type = await context.LookupTypes.FirstOrDefaultAsync(t => t.Code == code);
            if (type == null)
            {
                type = new LookupType { Code = code, Name = name };
                context.LookupTypes.Add(type);
                await context.SaveChangesAsync();
            }
            return type;
        }

        private static async Task GetOrCreateValueAsync(ApplicationDbContext context, int typeId, string name, string value, int order, string? valStr = null)
        {
            if (!await context.LookupValues.AnyAsync(v => v.LookupTypeId == typeId && v.Value == value))
            {
                context.LookupValues.Add(new LookupValue 
                { 
                    LookupTypeId = typeId, 
                    Name = name, 
                    Value = value, 
                    SortOrder = order 
                });
            }
        }
    }
}
