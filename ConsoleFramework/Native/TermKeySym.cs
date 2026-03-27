namespace ConsoleFramework.Native
{
    public enum TermKeySym
    {
        TERMKEY_SYM_UNKNOWN = -1,
        TERMKEY_SYM_NONE = 0,

        /* Special names in C0 */
        TERMKEY_SYM_BACKSPACE,
        TERMKEY_SYM_TAB,
        TERMKEY_SYM_ENTER,
        TERMKEY_SYM_ESCAPE,

        /* Special names in G0 */
        TERMKEY_SYM_SPACE,
        TERMKEY_SYM_DEL,

        /* Special keys */
        TERMKEY_SYM_UP,
        TERMKEY_SYM_DOWN,
        TERMKEY_SYM_LEFT,
        TERMKEY_SYM_RIGHT,
        TERMKEY_SYM_BEGIN,
        TERMKEY_SYM_FIND,
        TERMKEY_SYM_INSERT,
        TERMKEY_SYM_DELETE,
        TERMKEY_SYM_SELECT,
        TERMKEY_SYM_PAGEUP,
        TERMKEY_SYM_PAGEDOWN,
        TERMKEY_SYM_HOME,
        TERMKEY_SYM_END,

        /* Special keys from terminfo */
        TERMKEY_SYM_CANCEL,
        TERMKEY_SYM_CLEAR,
        TERMKEY_SYM_CLOSE,
        TERMKEY_SYM_COMMAND,
        TERMKEY_SYM_COPY,
        TERMKEY_SYM_EXIT,
        TERMKEY_SYM_HELP,
        TERMKEY_SYM_MARK,
        TERMKEY_SYM_MESSAGE,
        TERMKEY_SYM_MOVE,
        TERMKEY_SYM_OPEN,
        TERMKEY_SYM_OPTIONS,
        TERMKEY_SYM_PRINT,
        TERMKEY_SYM_REDO,
        TERMKEY_SYM_REFERENCE,
        TERMKEY_SYM_REFRESH,
        TERMKEY_SYM_REPLACE,
        TERMKEY_SYM_RESTART,
        TERMKEY_SYM_RESUME,
        TERMKEY_SYM_SAVE,
        TERMKEY_SYM_SUSPEND,
        TERMKEY_SYM_UNDO,

        /* Numeric keypad special keys */
        TERMKEY_SYM_KP0,
        TERMKEY_SYM_KP1,
        TERMKEY_SYM_KP2,
        TERMKEY_SYM_KP3,
        TERMKEY_SYM_KP4,
        TERMKEY_SYM_KP5,
        TERMKEY_SYM_KP6,
        TERMKEY_SYM_KP7,
        TERMKEY_SYM_KP8,
        TERMKEY_SYM_KP9,
        TERMKEY_SYM_KPENTER,
        TERMKEY_SYM_KPPLUS,
        TERMKEY_SYM_KPMINUS,
        TERMKEY_SYM_KPMULT,
        TERMKEY_SYM_KPDIV,
        TERMKEY_SYM_KPCOMMA,
        TERMKEY_SYM_KPPERIOD,
        TERMKEY_SYM_KPEQUALS,

        /* et cetera ad nauseum */
        TERMKEY_N_SYMS
    }
}
