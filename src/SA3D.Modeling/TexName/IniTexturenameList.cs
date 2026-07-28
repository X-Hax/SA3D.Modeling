using System;

namespace SA3D.Modeling.TexName
{
	[Serializable]
	internal class IniTexturenameList
	{
		public string Name { get; set; }
		public string TexnameArrayName { get; set; }
		public uint NumTextures { get; set; }
		public string[] TextureNames { get; set; }


		public IniTexturenameList(string name, string texnameArrayName, uint numTextures, string[] textureNames)
		{
			Name = name;
			TexnameArrayName = texnameArrayName;
			NumTextures = numTextures;
			TextureNames = textureNames;
		}
		public IniTexturenameList() : this(string.Empty, string.Empty, 0, []) { }
	}
}
