package com.neeeu.audiowalk;

import android.content.Context;
import android.content.Intent;

import androidx.annotation.IntDef;

import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;

// Command enumeration
// more info - http://blog.shamanland.com/2016/02/int-string-enum.html
@IntDef({Command.INVALID, Command.STOP, Command.START})
@Retention(RetentionPolicy.SOURCE)
@interface Command {

    int INVALID = -1;
    int STOP = 0;
    int START = 1;
    int PAUSE = 2;
}

public class MyIntentBuilder {

    public static MyIntentBuilder getInstance(Context context) {
        return new MyIntentBuilder(context);
    }


    public static final String KEY_MESSAGE = "msg";
    public static final String KEY_COMMAND = "cmd";
    private Context mContext;
    private String mMessage;
    private @Command int mCommandId = Command.INVALID;

    public MyIntentBuilder(Context context) {
        this.mContext = context;
    }

    public MyIntentBuilder setMessage(String message) {
        this.mMessage = message;
        return this;
    }

    public MyIntentBuilder setCommand(@Command int command) {
        this.mCommandId = command;
        return this;
    }

    public Intent build(Class theClass) {
        if (mContext != null)
        {
            Intent intent = new Intent(mContext, theClass);
            if (mCommandId != Command.INVALID) {
                intent.putExtra(KEY_COMMAND, mCommandId);
            }
            if (mMessage != null) {
                intent.putExtra(KEY_MESSAGE, mMessage);
            }
            return intent;
        }

        return null;
    }

    public static boolean containsCommand(Intent intent) {
        if (intent != null)
            return intent.getExtras().containsKey(KEY_COMMAND);
        return false;
    }

    public static boolean containsMessage(Intent intent) {
        if (intent != null)
            return intent.getExtras().containsKey(KEY_MESSAGE);
        return false;
    }

    public static @Command int getCommand(Intent intent) {
        final @Command int commandId = intent.getExtras().getInt(KEY_COMMAND);
        return commandId;
    }

    public static String getMessage(Intent intent) {
        return intent.getExtras().getString(KEY_MESSAGE);
    }
}
