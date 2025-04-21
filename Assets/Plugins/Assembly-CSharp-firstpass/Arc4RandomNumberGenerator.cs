using System.Collections.Generic;
using System.Security.Cryptography;

public class Arc4RandomNumberGenerator
{
	private class Arc4Stream
	{
		public byte i;

		public byte j;

		public byte[] s = new byte[256];

		public Arc4Stream()
		{
			for (int i = 0; i <= 255; i++)
			{
				s[i] = (byte)i;
			}
			this.i = 0;
			j = 0;
		}
	}

	private const int STIR_INCREMENT_CONST = 1600000;

	private static readonly Arc4RandomNumberGenerator instance = new Arc4RandomNumberGenerator();

	private Arc4Stream stream = new Arc4Stream();

	private int count;

	public int RandomNumber()
	{
		count -= 4;
		StirIfNeeded();
		return GetWord();
	}

	public void RandomValues(List<byte> result, int offset, int length)
	{
		StirIfNeeded();
		while (length-- != 0)
		{
			count--;
			StirIfNeeded();
			result[offset + length] = GetByte();
		}
	}

	private void AddRandomData(byte[] data)
	{
		stream.i--;
		for (int i = 0; i < 256; i++)
		{
			stream.i++;
			byte b = stream.s[stream.i];
			stream.j += (byte)(b + data[i % data.Length]);
			stream.s[stream.i] = stream.s[stream.j];
			stream.s[stream.j] = b;
		}
		stream.j = stream.i;
	}

	private void Stir()
	{
		byte[] array = new byte[128];
		CryptographicallyRandomValuesFromOS(array);
		AddRandomData(array);
		for (int i = 0; i < 256; i++)
		{
			GetByte();
		}
		count = 1600000;
	}

	private void StirIfNeeded()
	{
		if (count <= 0)
		{
			Stir();
		}
	}

	private byte GetByte()
	{
		stream.i++;
		byte b = stream.s[stream.i];
		stream.j += b;
		byte b2 = stream.s[stream.j];
		stream.s[stream.i] = b2;
		stream.s[stream.j] = b;
		return stream.s[(b + b2) & 0xFF];
	}

	private int GetWord()
	{
		return (GetByte() << 24) | (GetByte() << 16) | (GetByte() << 8) | GetByte();
	}

	public static int CryptographicallyRandomNumber()
	{
		return instance.RandomNumber();
	}

	public static void CryptographicallyRandomValues(List<byte> buffer, int offset, int length)
	{
		instance.RandomValues(buffer, offset, length);
	}

	private static void CryptographicallyRandomValuesFromOS(byte[] buffer)
	{
		RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider();
		rNGCryptoServiceProvider.GetBytes(buffer);
	}
}
