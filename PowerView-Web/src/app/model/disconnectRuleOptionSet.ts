import { DisconnectControlName } from './disconnectControlName';
import { DisconnectEvaluationName } from './disconnectEvaluationName';

export class DisconnectRuleOptionSet {
    disconnectControlItems: DisconnectControlName[];
    evaluationItems: DisconnectEvaluationName[];

    constructor() {
        this.disconnectControlItems = [];
        this.evaluationItems = [];
    }
}
