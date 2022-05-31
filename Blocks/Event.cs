﻿using ScratchScript.Blocks.Builders;
using ScratchScript.Wrapper;

namespace ScratchScript.Blocks;

public class Event
{
	public static Block WhenFlagClicked()
	{
		return new BlockBuilder()
			.IsShadow(false)
			.WithOpcode("event_whenflagclicked")
			.WithId("FlagClicked");
	}
}