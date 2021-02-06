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
        QAInProgress = 4,
        ReadyForShadowQA = 5,
        ShadowQAInProgress = 6,
        ReadyForPosting = 7,
        PostingCompleted = 8,
        QARejected = 9,
        ShadowQARejected = 10,
        CoderRejected = 11
    }
}
