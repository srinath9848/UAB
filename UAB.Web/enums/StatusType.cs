using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UAB.enums
{
    public enum StatusType
    {
        ReadyForCoding = 1,
        CodingInProgress = 2,
        CodingCompleted = 3,
        ReadyforQA = 4,
        QAInProgress = 5,
        QACompleted = 6,
        QARejected = 7,
        ReadyforShadowQA = 8,
        ShadowQAInProgress = 9,
        ShadowQACompleted = 10,
        ShadowQARejected = 11,
        CoderRebutted = 12,
        QARebutted = 13,
        ToCoderCorrection = 14,
        ReadyforPosting = 15,
        PostingCompleted = 16
    }
}
