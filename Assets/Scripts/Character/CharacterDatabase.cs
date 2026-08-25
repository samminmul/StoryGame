using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Scriptable Objects/CharacterDatabase")]
public class CharacterDatabase : ScriptableObject
{
    public List<CharacterData> characterDataList;
    private Dictionary<string, CharacterData> m_CharacterList = new Dictionary<string, CharacterData>();

    public void AddCharacterData(CharacterData characterData)
    {
        if (!m_CharacterList.ContainsKey(characterData.characterID))
        {
            m_CharacterList.Add(characterData.characterID, characterData);
        }
    }

    public CharacterData GetCharacterData(string characterID)
    {
        if (m_CharacterList.TryGetValue(characterID, out CharacterData characterData))
        {
            return characterData;
        }
        else
        {
            Debug.LogError($"Character ID를 찾을 수 없습니다: {characterID}");
            return null;
        }
    }
}
