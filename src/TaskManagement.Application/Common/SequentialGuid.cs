using System.Security.Cryptography;

namespace TaskManagement.Application.Common;

/// <summary>
/// Генерация последовательных GUID по схеме UUIDv7 (RFC 9562):
/// первые 48 бит — Unix-время в мс, далее версия/вариант и случайные биты.
/// Монотонность по времени даёт локальность в btree-индексе PostgreSQL
/// (в отличие от случайного v4), сохраняя при этом неугадываемость.
/// </summary>
public static class SequentialGuid
{
    public static Guid NewGuid()
    {
        Span<byte> bytes = stackalloc byte[16];

        // 48-битная метка времени (big-endian) в первых 6 байтах.
        long unixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        bytes[0] = (byte)(unixMs >> 40);
        bytes[1] = (byte)(unixMs >> 32);
        bytes[2] = (byte)(unixMs >> 24);
        bytes[3] = (byte)(unixMs >> 16);
        bytes[4] = (byte)(unixMs >> 8);
        bytes[5] = (byte)unixMs;

        // Остальные 10 байт — криптослучайные.
        RandomNumberGenerator.Fill(bytes[6..]);

        // Версия 7 (старший полубайт байта 6) и вариант RFC 4122 (старшие биты байта 8).
        bytes[6] = (byte)((bytes[6] & 0x0F) | 0x70);
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);

        // bigEndian: true — каноническое представление совпадает с порядком сравнения uuid в PostgreSQL.
        return new Guid(bytes, bigEndian: true);
    }
}
