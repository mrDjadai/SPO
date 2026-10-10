public class Test5_IfInsideCase {

    public static String classify(int code, int value) {
        String result;

        switch (code) {
            case 1:
                if (value > 0) {
                    result = "positive";
                } else {
                    result = "non-positive";
                }
                break;

            case 2:
                result = "two";
                break;

            case 3:
                if (value == 0) {
                    if (value < 10) {
                        result = "small zero";
                    } else {
                        result = "big zero";
                    }
                } else {
                    result = "non-zero";
                }
                break;

            default:
                result = "unknown";
                break;
        }

        return result;
    }

    public static void main(String[] args) {
        System.out.println(classify(1, 5));
        System.out.println(classify(2, 0));
        System.out.println(classify(3, 0));
    }
}