using System;
using System.Collections.Generic;

public class OptionDataTables
{
	public static readonly Dictionary<Option, Type> s_typeMap = new Dictionary<Option, Type>
	{
		{
			Option.GFX_QUALITY,
			typeof(int)
		},

		{
			Option.GFX_TARGET_FRAME_RATE,
			typeof(int)
		},
		
		{
			Option.INTRO,
			typeof(bool)
		},

		{
			Option.SOUND,
			typeof(bool)
		},

		{
			Option.BACKGROUND_SOUND,
			typeof(bool)
		},
	};
	public static readonly Dictionary<Option, object> s_defaultsMap = new Dictionary<Option, object>
	{
		{
			Option.GFX_QUALITY,
			1
		},
		
		{
			Option.GFX_TARGET_FRAME_RATE,
			30
		},
		
		{
			Option.INTRO,
			true
		},
		
		{
			Option.SOUND,
			true
		},
		
		{
			Option.BACKGROUND_SOUND,
			true
		},
	};
}
