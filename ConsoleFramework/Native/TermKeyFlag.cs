using System;

namespace ConsoleFramework.Native
{
    [Flags]
    public enum TermKeyFlag
    {
        TERMKEY_FLAG_NOINTERPRET = 1 << 0, /* Do not interpret C0//DEL codes if possible */
        TERMKEY_FLAG_CONVERTKP = 1 << 1, /* Convert KP codes to regular keypresses */
        TERMKEY_FLAG_RAW = 1 << 2, /* Input is raw bytes, not UTF-8 */
        TERMKEY_FLAG_UTF8 = 1 << 3, /* Input is definitely UTF-8 */
        TERMKEY_FLAG_NOTERMIOS = 1 << 4, /* Do not make initial termios calls on construction */
        TERMKEY_FLAG_SPACESYMBOL = 1 << 5, /* Sets TERMKEY_CANON_SPACESYMBOL */
        TERMKEY_FLAG_CTRLC = 1 << 6, /* Allow Ctrl-C to be read as normal, disabling SIGINT */
        TERMKEY_FLAG_EINTR = 1 << 7 /* Return ERROR on signal (EINTR) rather than retry */
    }
}
