using System;
using System.IO;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class ScriptableObjectViewPrefs<VIEWMODEL> : ScriptableObject where VIEWMODEL : ScriptableObject
	{
		private static readonly string k_Extension = ".pref";

		private ScriptableObject m_ActiveAsset;

		private VIEWMODEL m_ActiveViewModel;

		public ScriptableObject activeAsset
		{
			get
			{
				return this.m_ActiveAsset;
			}
		}

		public VIEWMODEL activeViewModel
		{
			get
			{
				return this.m_ActiveViewModel;
			}
		}

		public void SetActiveAsset(ScriptableObject asset)
		{
			if (!(this.m_ActiveAsset == asset))
			{
				if (this.m_ActiveAsset != null)
				{
					this.Save(this.m_ActiveAsset, this.m_ActiveViewModel);
				}
				this.Load(asset);
			}
		}

		public VIEWMODEL CreateNewViewModel()
		{
			VIEWMODEL result = ScriptableObject.CreateInstance<VIEWMODEL>();
			result.set_hideFlags(result.get_hideFlags() | 61);
			return result;
		}

		private static string AssetKey(Object asset)
		{
			string result;
			if (asset == null)
			{
				result = string.Empty;
			}
			else
			{
				result = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
			}
			return result;
		}

		protected void Save(ScriptableObject asset, VIEWMODEL viewData)
		{
			if (!(asset == null) && !(viewData == null))
			{
				string text = ScriptableObjectViewPrefs<VIEWMODEL>.AssetKey(asset);
				if (!string.IsNullOrEmpty(text))
				{
					string path = Application.get_dataPath() + "/../" + this.GetFilePath();
					if (!Directory.Exists(path))
					{
						Directory.CreateDirectory(path);
					}
					InternalEditorUtility.SaveToSerializedFileAndForget(new VIEWMODEL[]
					{
						viewData
					}, this.GetProjectRelativePath(text), true);
				}
			}
		}

		private string GetProjectRelativePath(string file)
		{
			return this.GetFilePath() + "/" + file + ScriptableObjectViewPrefs<VIEWMODEL>.k_Extension;
		}

		private void Load(ScriptableObject asset)
		{
			VIEWMODEL vIEWMODEL = (VIEWMODEL)((object)null);
			string text = ScriptableObjectViewPrefs<VIEWMODEL>.AssetKey(asset);
			if (!string.IsNullOrEmpty(text))
			{
				Object[] array = InternalEditorUtility.LoadSerializedFileAndForget(this.GetProjectRelativePath(text));
				if (array.Length > 0)
				{
					vIEWMODEL = (array[0] as VIEWMODEL);
					vIEWMODEL.set_hideFlags(vIEWMODEL.get_hideFlags() | 61);
				}
			}
			this.m_ActiveAsset = asset;
			VIEWMODEL arg_79_1;
			if ((arg_79_1 = vIEWMODEL) == null)
			{
				arg_79_1 = this.CreateNewViewModel();
			}
			this.m_ActiveViewModel = arg_79_1;
		}

		private string GetFilePath()
		{
			Type type = base.GetType();
			object[] customAttributes = type.GetCustomAttributes(true);
			object[] array = customAttributes;
			string result;
			for (int i = 0; i < array.Length; i++)
			{
				object obj = array[i];
				if (obj is FilePathAttribute)
				{
					FilePathAttribute filePathAttribute = obj as FilePathAttribute;
					result = filePathAttribute.get_filepath();
					return result;
				}
			}
			result = "Library";
			return result;
		}
	}
}
