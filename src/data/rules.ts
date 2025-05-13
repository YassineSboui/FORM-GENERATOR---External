export interface MyRules {
  required: (value: any) => string;
  timeBetween: (value: any, params: []) => string;
  timeBefore: (value: any, params: []) => string;
  timeAfter: (value: any, params: []) => string;
  dateBetween: (value: any, params: []) => string;
  dateBefore: (value: any, params: []) => string;
  dateAfter: (value: any, params: []) => string;
  dateIsNot: (value: any, params: []) => string;
  disabledDateRange : (value: any, params: []) => string;
  disabledMonthDays : (value: any, params: []) => string;
  disabledWeekDays : (value: any, params: []) => string;
}

export const useMyRules: MyRules = {
  required: (value) => (value ? "true" : "Ce champ est requis"),
  timeBetween: (value: any, params: any) => {
    if (params.length == 0) return "true";
    const Start = params[0];
    const End = params[1];
    if (Start > value || End < value) return "The time isn't in range";
    return "true";
  },
  timeBefore: (value: any, params: any) => {
    if (value > params[0])
      return (
        "The time must be before " +
        params[0].getHours() +
        ":" +
        params[0].getMinutes()
      );
    return "true";
  },
  timeAfter: (value: any, params: any) => {
    if (value < params[0])
      return (
        "The time must be after " +
        params[0].getHours() +
        ":" +
        params[0].getMinutes()
      );
    return "true";
  },
  dateBetween: (value: any, params: any) => {
    if (params.length == 0) return "true";
    const Start = params[0];
    const End = params[1];
    if (Start > value || End < value) return "The date isn't in range";
    return "true";
  },
  dateBefore: (value: any, params: any) => {
    if (value > params[0])
      return (
        "The date must be before " +
        params[0].getDate() +
        "/" +
        params[0].getMonth() +
        "/" +
        params[0].getFullYear()
      );
    return "true";
  },
  dateAfter: (value: any, params: any) => {
    if (value < params[0])
      return (
        "The date must be after " +
        params[0].getDate() +
        "/" +
        params[0].getMonth() +
        "/" +
        params[0].getFullYear()
      );
    return "true";
  },
  dateIsNot: (value: any, params: []) => {
    params.forEach((val : any) => {
      if(value == val )
        return 'Date not valid'
    })
    return 'true'
  },
  disabledDateRange:(value: any, params: any) => {
    if (value >= params[0] && value <= params[1]) {
        return 'true'; // Value is within the range
    }
    return 'Date not valid';
  },
  disabledWeekDays: (value: any, params: []) => {
    return  'true';
  },
  disabledMonthDays : (value: any, params: []) => {
    return  'true';
  },
};
