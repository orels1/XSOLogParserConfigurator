// https://github.com/nnaaa-vr/XSOverlay-VRChat-Parser/blob/main/XSOverlay%20VRChat%20Parser/Helpers/Annotation.cs
using System;

namespace XSOParserConfig
{
  [AttributeUsage(AttributeTargets.Property)]
  public class Annotation : Attribute
  {
    public string description;
    public string groupDescription;
    public bool startsGroup;
    public Annotation(string _description, bool _startsGroup = false, string _groupDescription = "")
    {
      this.description = _description;
      this.startsGroup = _startsGroup;
      this.groupDescription = _groupDescription;
    }
  }
}