using UnityEngine;
using UnityEditor;

public class NodeHelper : EditorWindow {

    public enum NodeHelperWindowType
    {
        help = 1,
        about = 2
    }

    string note;
    GUIStyle style;

    public static NodeHelperWindowType type = NodeHelperWindowType.help;

    void OnEnable()
    {
        style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;
        switch (type)
        {
            case NodeHelperWindowType.help:
                title = "使用手册";
                note = "\n\t\t    规则:" +
                       "\n1. 绑定对象节点不可删除,不可有父节点." +
                       "\n2. F1快速跳转至根节点." +
                       "\n3. 左右方向键依次跳转至各个节点." +
                       "\n4. 拖动时按住Shift或Control可以加快拖动速度." +
                       "\n5. Shift + S 主动保存当前编辑数据." +
                       "\n6. Ctrl+Shift+Z/Y 回撤/重做." +
                       "\n7. Delete 删除当前节点." +
                       "\n8. Shift + Del 删除当前节点以及其所有子孙节点." + 
                       "\n9. 按住Alt并拖动鼠标可以选中多个节点, 支持移动和删除操作.";
                break;
            case NodeHelperWindowType.about:
                title = "关于";
                note = "\n\t               关于:\n" +
                       "\n   当前版本不可避免存在很多不足和bug,"+
                       "\n   希望大家在使用过程中及时反馈给制作人."+
                       "\n\n\n\t\t\t  制作人: 韩君松";
                break;
        }
    }

    void OnGUI()
    {
        note = EditorGUILayout.TextArea(note, style);
    }
}