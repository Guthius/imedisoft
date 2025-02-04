using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class SmsBlockPhones
{
    public static void Insert(SmsBlockPhone smsBlockPhone)
    {
        SmsBlockPhoneCrud.Insert(smsBlockPhone);
    }
}