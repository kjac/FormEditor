addFormEditorCondition("my.length", function (rule, fieldValue, $form) {
  // rule condition is fulfilled if the field value is null or the field value length is less than the value defined for lessThan
  return (fieldValue == null || fieldValue.length < rule.condition.lessThan);
});
